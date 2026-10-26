using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Order;

public class ConfirmPaymentHandler : IRequestHandler<ConfirmPaymentCommand, PaymentConfirmation>
{
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ConfirmPaymentHandler> _logger;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailBackgroundService _emailBackgroundService;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmPaymentHandler(
        IPaymentRepository paymentRepository,
        IPaymentService paymentService,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        ILogger<ConfirmPaymentHandler> logger,
        IEmailTemplateService emailTemplateService,
        IEmailBackgroundService emailBackgroundService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _logger = logger;
        _emailTemplateService = emailTemplateService;
        _emailBackgroundService = emailBackgroundService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentConfirmation> Handle(ConfirmPaymentCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "Payment confirmation started: PaymentIntentId {PaymentIntentId}",
            command.PaymentIntentId
        );

        // 1. Находим платёж и проверяем владельца.
        var payment = await _paymentRepository.GetByTransactionIdAsync(command.PaymentIntentId, ct);
        if (payment == null)
        {
            _logger.LogWarning(
                "Payment confirmation failed: Payment not found. PaymentIntentId {PaymentIntentId}",
                command.PaymentIntentId
            );

            throw new DomainException("Payment not found");
        }

        if (payment.UserId != command.UserId)
            throw new UnauthorizedAccessException("This payment doesn't belong to you");

        // 2. Идемпотентность: повторный confirm уже оплаченного платежа.
        if (payment.Status == PaymentStatus.Paid)
        {
            return new PaymentConfirmation
            {
                Success = true,
                TransactionId = payment.ExternalTransactionId ?? payment.TransactionId
            };
        }

        if (payment.Status != PaymentStatus.Pending)
            throw new DomainException($"Cannot confirm payment in status '{payment.Status}'.");

        // 3. Предварительная проверка заказа до обращения к платёжному шлюзу.
        var orderBeforeConfirmation = await _orderRepository.GetOrder(payment.OrderId, ct);
        if (orderBeforeConfirmation == null)
            throw new DomainException("Order not found");

        if (orderBeforeConfirmation.UserId != command.UserId)
            throw new UnauthorizedAccessException("This order doesn't belong to you");

        if (orderBeforeConfirmation.Status != OrderStatus.Pending)
            throw new DomainException(
                $"Cannot pay for an order in status '{orderBeforeConfirmation.Status}'.");

        var user = await _userRepository.GetByIdAsync(payment.UserId, ct);
        if (user == null)
            throw new DomainException("User isn't found.");

        // 4. Проверяем платёж через внешний сервис БЕЗ удержания блокировок БД.
        var result = await _paymentService.ConfirmPaymentAsync(command.PaymentIntentId, ct);
        if (result == null)
            throw new DomainException("Confirmation is null");

        if (!result.Success)
        {
            _logger.LogWarning(
                "Payment confirmation failed: OrderId {OrderId}, PaymentIntentId {PaymentIntentId}, Error {Error}",
                payment.OrderId,
                command.PaymentIntentId,
                result.ErrorMessage
            );

            // Не помечаем платёж оплаченным и не сохраняем failed-состояние:
            // пользователь может повторить подтверждение.
            return result;
        }

        var externalTransactionId = result.TransactionId ?? command.PaymentIntentId;

        // 5. Короткая транзакция: блокируем платёж, заказ и товары, затем
        //    переводим их в Paid одной атомарной операцией.
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var lockedPayment = await _paymentRepository
                .GetByTransactionIdForUpdateAsync(command.PaymentIntentId, ct);

            if (lockedPayment == null)
                throw new DomainException("Payment not found");

            if (lockedPayment.Status == PaymentStatus.Paid)
            {
                // Параллельный запрос уже подтвердил платёж.
                await _unitOfWork.CommitTransactionAsync(ct);

                return new PaymentConfirmation
                {
                    Success = true,
                    TransactionId = lockedPayment.ExternalTransactionId ?? externalTransactionId
                };
            }

            if (lockedPayment.Status != PaymentStatus.Pending)
                throw new DomainException(
                    $"Cannot confirm payment in status '{lockedPayment.Status}'.");

            var order = await _orderRepository.GetOrderForUpdateAsync(lockedPayment.OrderId, ct);
            if (order == null)
                throw new DomainException("Order not found");

            if (order.UserId != command.UserId)
                throw new UnauthorizedAccessException("This order doesn't belong to you");

            if (order.Status != OrderStatus.Pending)
                throw new DomainException(
                    $"Cannot pay for an order in status '{order.Status}'.");

            var productIds = order.Items
                .Select(i => i.ProductId)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            var products = await _productRepository.GetByIdsForUpdateAsync(productIds, ct);
            if (products.Count != productIds.Count)
                throw new DomainException("Some products from the order were not found");

            lockedPayment.MarkAsPaid(externalTransactionId);
            order.MarkAsPaid();

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            _logger.LogInformation(
                "Payment confirmed successfully: OrderId {OrderId}, PaymentIntentId {PaymentIntentId}, TransactionId {TransactionId}, Amount {Amount}",
                order.Id,
                command.PaymentIntentId,
                externalTransactionId,
                lockedPayment.Amount
            );

            // 6. Email отправляем только после успешного COMMIT.
            try
            {
                var email = _emailTemplateService.CreatePaymentConfirmationEmail(
                    order,
                    user,
                    lockedPayment,
                    command.BaseUrl
                );

                await _emailBackgroundService.Enqueue(email);
                _logger.LogInformation("Payment confirmation email queued for {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue payment confirmation for {Email}", user.Email);
            }

            return new PaymentConfirmation
            {
                Success = true,
                TransactionId = externalTransactionId
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
