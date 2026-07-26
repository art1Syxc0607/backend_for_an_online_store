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

        // 2. Проверяем, что заказ принадлежит пользователю
        if (order.UserId != command.UserId)
            throw new UnauthorizedAccessException("This order doesn't belong to you");

        // 3. Проверяем, что заказ не оплачен и не отменен
        if (order.Status == OrderStatus.Paid)
            throw new DomainException("Order is already paid");
        if (order.Status == OrderStatus.Cancelled)
            throw new DomainException("Cannot pay for a cancelled order");

        // 4. Инициируем оплату через внешний сервис
        var result = await _paymentService.InitiatePaymentAsync(
            command.OrderId,
            command.Method,
            ct
        );

        // 5. Если оплата успешно инициирована — сохраняем Payment в БД
        if (result.Success)
        {
            // Сохраняем информацию о платеже
            order.MarkAsPaid(); // ← метод в сущности Order

            // Сохраняем Payment (если есть такая сущность)
            //var payment = new Payment
            //{
            //    OrderId = order.Id,
            //    Amount = order.TotalAmount,
            //    Status = PaymentStatus.Pending,
            //    TransactionId = result.PaymentIntentId,
            //    PaymentMethod = command.Method,
            //    CreatedAt = DateTime.UtcNow
            //};

            var payment = new Payment(order.Id, order.TotalAmount, command.Method);
            await _paymentRepository.AddAsync(payment, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return result;
    }
}