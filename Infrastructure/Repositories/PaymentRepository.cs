using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Payment?> GetByOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == orderId, ct);
    }

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, ct);
    }

    public async Task<Payment?> GetByExternalTransactionIdAsync(string externalTransactionId, CancellationToken ct = default)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.ExternalTransactionId == externalTransactionId, ct);
    }

    public async Task<List<Payment>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .Where(p => p.Order.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
    {
        await _context.Payments.AddAsync(payment, ct);
    }

    public Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        _context.Payments.Update(payment);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        return await _context.Payments.AnyAsync(p => p.OrderId == orderId, ct);
    }

    public async Task<List<Payment>> GetPendingPaymentsAsync(DateTime olderThan, CancellationToken ct = default)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .Where(p => p.Status == PaymentStatus.Pending && p.CreatedAt < olderThan)
            .ToListAsync(ct);
    }
}
