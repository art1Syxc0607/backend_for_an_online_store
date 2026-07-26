using Domain.Enums;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class Payment
{
    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string? TransactionId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public virtual Order Order { get; private set; }

    private Payment() { }

    public Payment(int orderId, decimal amount, PaymentMethod method, string? transactionId = null)
    {
        OrderId = orderId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
        TransactionId = transactionId;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid()
    {
        if (Status == PaymentStatus.Paid)
            throw new DomainException("Payment is already paid");
        Status = PaymentStatus.Paid;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string? reason = null)
    {
        Status = PaymentStatus.Failed;
    }

    public void MarkAsRefunded()
    {
        Status = PaymentStatus.Refunded;
    }
}