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
        // An order can have several payment attempts (pending -> failed -> paid),
        // so return the most recent one instead of an arbitrary row.
        return await _context.Payments
            .Include(p => p.Order)
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, ct);
    }

    public async Task<Payment?> GetByTransactionIdForUpdateAsync(
        string transactionId,
        CancellationToken ct = default)
    {
        var payment = await _context.Payments
            .FromSqlInterpolated(
                $@"SELECT *, xmin FROM ""Payments""
                   WHERE ""TransactionId"" = {transactionId}
                   FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        // A tracking query does not refresh an entity that is already tracked.
        if (payment != null)
            await _context.Entry(payment).ReloadAsync(ct);

        return payment;
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
        // The payment is already tracked by EF.
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        return await _context.Payments.AnyAsync(p => p.OrderId == orderId, ct);
    }

    public async Task<Payment?> GetPendingByOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        return await _context.Payments
            .Where(p => p.OrderId == orderId && p.Status == PaymentStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Payment?> GetPendingByOrderIdForUpdateAsync(
        int orderId,
        CancellationToken ct = default)
    {
        // Status is stored as an int, so Pending == 0. The row is locked until
        // the transaction ends, which serialises concurrent payment initiation.
        var payment = await _context.Payments
            .FromSqlInterpolated(
                $@"SELECT *, xmin FROM ""Payments""
                   WHERE ""OrderId"" = {orderId} AND ""Status"" = {(int)PaymentStatus.Pending}
                   ORDER BY ""CreatedAt"" DESC
                   LIMIT 1
                   FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        // A tracking query does not refresh an entity that is already tracked.
        if (payment != null)
            await _context.Entry(payment).ReloadAsync(ct);

        return payment;
    }

    public async Task<List<Payment>> GetPendingPaymentsAsync(DateTime olderThan, CancellationToken ct = default)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .Where(p => p.Status == PaymentStatus.Pending && p.CreatedAt < olderThan)
            .ToListAsync(ct);
    }
}
