using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Cleanup;


public class ExpiredOrdersCleanupTask : ICleanupTask
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
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
        IUserRepository userRepository,
        IEmailBackgroundService emailService,
        IEmailTemplateService emailTemplate,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IOptions<ExpiredOrdersOptions> options,
        ILogger<ExpiredOrdersCleanupTask> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _emailTemplate = emailTemplate;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(CancellationToken ct)
    {
        // ✅ Читаем baseUrl из конфигурации
        var baseUrl = _configuration["App:BaseUrl"]
            ?? throw new InvalidOperationException(
                "App:BaseUrl is not configured in appsettings.json");

        var threshold = DateTime.UtcNow.AddMinutes(-_options.PaymentTimeoutMinutes);
        var totalCancelled = 0;
        const int batchSize = 100;


        while (!ct.IsCancellationRequested)
        {
            var batch = await _orderRepository.GetExpiredPendingOrdersAsync(
                threshold, batchSize, ct); // заказ с его элементами и продуктами 

            if (!batch.Any()) break;

            foreach (var order in batch)
            {
                try
                {
                    order.Cancel();  // ✅ один раз
                                      // + Release reserve of products

                    var user = await _userRepository.GetByIdAsync(order.UserId, ct);
                    if (user != null)
                    {
                        var email = _emailTemplate.CreateOrderCancelledByTimeoutEmail(
                            order, user, baseUrl);
                        await _emailService.Enqueue(email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cancel order {OrderId}", order.Id);
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
            totalCancelled += batch.Count;

            // Если загрузили меньше batchSize — значит это была последняя партия
            if (batch.Count < batchSize) break;
        }

        return totalCancelled;
    }
}