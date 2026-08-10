using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order;

public class ConfirmPaymentHandler : IRequestHandler<ConfirmPaymentCommand, PaymentConfirmation>
{
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmPaymentHandler(IPaymentRepository paymentRepository, IPaymentService paymentService,
        IOrderRepository orderRepository, ICacheService cacheService, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentConfirmation> Handle(ConfirmPaymentCommand command, CancellationToken ct)
    {
        // 1. Получаем платеж по transactionId
        var payment = await _paymentRepository.GetByTransactionIdAsync(command.PaymentIntentId, ct);
        if (payment == null)
            throw new DomainException("Payment not found");

        // 2. Проверяем статус платежа через внешний сервис
        var result = await _paymentService.ConfirmPaymentAsync(command.PaymentIntentId, ct);

        if (!result.Success)
        {
            payment.MarkAsFailed();
            await _unitOfWork.SaveChangesAsync(ct);
            return result;
        }

        // 3. Обновляем payment
        payment.MarkAsPaid(result.TransactionId);

        // 4. Обновляем order
        var order = await _orderRepository.GetOrder(payment.OrderId, ct);
        if (order == null) throw new DomainException("Order not found");
        order.MarkAsPaid();

        await _unitOfWork.SaveChangesAsync(ct);

        // ✅ Очищаем кэш товаров
        await _cacheService.RemoveByPrefix("products:");

        return result;
    }
}