using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
        => _context = context;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            throw new InvalidOperationException("A database transaction is already active.");

        _transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        var transaction = _transaction
            ?? throw new InvalidOperationException("There is no active database transaction.");

        // Clear the field before committing so a failed commit is never
        // accidentally rolled back after the database already accepted it.
        _transaction = null;

        try
        {
            await transaction.CommitAsync(ct);
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        var transaction = _transaction;
        if (transaction == null)
            return;

        _transaction = null;

        try
        {
            await transaction.RollbackAsync(ct);
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        var transaction = _transaction;
        if (transaction == null)
            return;

        _transaction = null;

        try
        {
            await transaction.RollbackAsync();
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }
}
