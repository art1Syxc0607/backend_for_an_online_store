using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Enums;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order;

public class InitiatePaymentHandler : IRequestHandler<InitiatePaymentCommand, PaymentResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InitiatePaymentHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IPaymentService paymentService,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _paymentRepository = paymentRepository;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentResult> Handle(InitiatePaymentCommand command, CancellationToken ct)
    {
        // 1. Проверяем заказ
        var order = await _orderRepository.GetOrder(command.OrderId, ct);
        if (order == null)
            throw new DomainException("Order not found");

        if (order.UserId != command.UserId)
            throw new UnauthorizedAccessException("This order doesn't belong to you");

        if (order.Status == OrderStatus.Paid)
            throw new DomainException("Order is already paid");
        if (order.Status == OrderStatus.Cancelled)
            throw new DomainException("Cannot pay for a cancelled order");

        // 2. Инициируем оплату через внешний сервис
        var result = await _paymentService.InitiatePaymentAsync(command.OrderId, order.TotalAmount, command.Method, ct);

        if (!result.Success)
            return result;

        // 3. Создаем Payment со статусом Pending (НЕ помечаем заказ как оплаченный!)
        var payment = new Payment(
            order.Id,
            order.TotalAmount,
            command.Method,
            result.PaymentIntentId
        );

        await _paymentRepository.AddAsync(payment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return result;
    }
}