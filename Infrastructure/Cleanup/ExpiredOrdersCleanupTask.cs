using Application.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Infrastructure.Options;

namespace Infrastructure.Cleanup;

public class ExpiredOrdersCleanupTask : ICleanupTask
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEmailBackgroundService _emailService;
    private readonly IEmailTemplateService _emailTemplate;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ExpiredOrdersOptions _options;
    private readonly ILogger<ExpiredOrdersCleanupTask> _logger;

    public string Name => "ExpiredOrders";
    public TimeSpan Interval => TimeSpan.FromMinutes(_options.CheckIntervalMinutes);

    public ExpiredOrdersCleanupTask(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IEmailBackgroundService emailService,
        IEmailTemplateService emailTemplate,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IOptions<ExpiredOrdersOptions> options,
        ILogger<ExpiredOrdersCleanupTask> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _emailService = emailService;
        _emailTemplate = emailTemplate;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(CancellationToken ct)
    {
        var baseUrl = _configuration["App:BaseUrl"]
            ?? throw new InvalidOperationException(
                "App:BaseUrl is not configured in appsettings.json");

        var threshold = DateTime.UtcNow.AddMinutes(-_options.PaymentTimeoutMinutes);
        var totalCancelled = 0;
        var batchSize = Math.Max(1, _options.BatchSize);

        while (!ct.IsCancellationRequested)
        {
            var batchIds = await _orderRepository.GetExpiredPendingOrderIdsAsync(
                threshold, batchSize, ct);

            if (!batchIds.Any())
                break;

            var cancelledInBatch = 0;

            foreach (var candidateId in batchIds)
            {
                if (ct.IsCancellationRequested)
                    break;

                await _unitOfWork.BeginTransactionAsync(ct);

                try
                {
                    // Re-read and lock the order: another worker or a payment
                    // confirmation may have changed it after the batch query.
                    var order = await _orderRepository.GetOrderForUpdateAsync(candidateId, ct);
                    if (order == null ||
                        order.Status != OrderStatus.Pending ||
                        order.CreatedAt >= threshold)
                    {
                        await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                        _unitOfWork.DiscardPendingChanges();
                        continue;
                    }

                    // Order.Cancel releases product reservations; lock product
                    // rows in ascending Id order before mutating them.
                    var productIds = order.Items
                        .Select(i => i.ProductId)
                        .Distinct()
                        .OrderBy(id => id)
                        .ToList();

                    var products = await _productRepository.GetByIdsForUpdateAsync(productIds, ct);
                    if (products.Count != productIds.Count)
                        throw new DomainException("Some products from the order were not found");

                    order.Cancel();

                    await _unitOfWork.SaveChangesAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);

                    // Commit succeeded: detach the tracked graph so the next
                    // order starts clean. The email below only reads in-memory data.
                    _unitOfWork.DiscardPendingChanges();

                    cancelledInBatch++;
                    totalCancelled++;

                    // Email only after the cancellation is committed.
                    try
                    {
                        // Loaded together with the order by GetOrderForUpdateAsync.
                        var user = order.User;
                        if (user != null)
                        {
                            var email = _emailTemplate.CreateOrderCancelledByTimeoutEmail(
                                order, user, baseUrl);
                            await _emailService.Enqueue(email);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to queue timeout cancellation email for order {OrderId}",
                            order.Id);
                    }
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                    _logger.LogError(ex, "Failed to cancel order {OrderId}", candidateId);

                    _unitOfWork.DiscardPendingChanges();
                }
            }

            // If nothing could be cancelled, the same rows would be fetched
            // forever (e.g. persistent product/reservation inconsistency).
            if (cancelledInBatch == 0)
                break;

            if (batchIds.Count < batchSize)
                break;
        }

        return totalCancelled;
    }
}
