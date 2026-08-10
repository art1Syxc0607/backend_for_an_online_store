using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class Payment
{
    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string TransactionId { get; private set; }          // наш внутренний ID
    public string? ExternalTransactionId { get; private set; } // ID от платежного шлюза
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    public Order Order { get; private set; }

    private Payment() { }

    public Payment(int orderId, decimal amount, PaymentMethod method, string? transactionId = null)
    {
        OrderId = orderId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
        TransactionId = transactionId ?? $"pay_{Guid.NewGuid():N}";
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(string externalTransactionId)
    {
        if (Status == PaymentStatus.Paid)
            throw new DomainException("Payment is already paid");

        if (Status == PaymentStatus.Refunded)
            throw new DomainException("Cannot mark refunded payment as paid");

        Status = PaymentStatus.Paid;
        ExternalTransactionId = externalTransactionId;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string? errorMessage = null)
    {
        if (Status == PaymentStatus.Paid)
            throw new DomainException("Cannot fail already paid payment");

        Status = PaymentStatus.Failed;
        ErrorMessage = errorMessage;
    }

    public void MarkAsRefunded()
    {
        if (Status != PaymentStatus.Paid)
            throw new DomainException("Only paid payments can be refunded");

        Status = PaymentStatus.Refunded;
    }
}