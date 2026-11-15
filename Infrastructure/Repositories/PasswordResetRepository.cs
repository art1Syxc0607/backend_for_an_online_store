// Infrastructure/Data/Repositories/PasswordResetRepository.cs
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly AppDbContext _context;

    public PasswordResetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetCode?> GetLatestByUserIdAsync(
        int userId,
        CancellationToken ct = default)
    {
        // ✅ Получаем последний активный код
        return await _context.PasswordResetCodes
            .Where(x => x.UserId == userId)
            .Where(x => x.UsedAt == null)              // Не использован
            .Where(x => x.ExpiresAt > DateTime.UtcNow) // Не просрочен
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PasswordResetCode?> GetLatestActiveForUpdateAsync(
        int userId,
        CancellationToken ct = default)
    {
        // The row stays locked until the transaction ends, so a concurrent
        // reset request waits here and then sees UsedAt != null.
        var now = DateTime.UtcNow;
        var code = await _context.PasswordResetCodes
            .FromSqlInterpolated(
                $@"SELECT *, xmin FROM ""PasswordResetCodes""
                   WHERE ""UserId"" = {userId}
                     AND ""UsedAt"" IS NULL
                     AND ""ExpiresAt"" > {now}
                   ORDER BY ""CreatedAt"" DESC
                   LIMIT 1
                   FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        // A tracking query does not refresh an entity that is already tracked.
        if (code != null)
            await _context.Entry(code).ReloadAsync(ct);

        return code;
    }

    public Task<int> IncrementAttemptsAsync(int codeId, CancellationToken ct = default)
    {
        // Single UPDATE ... SET "AttemptsCount" = "AttemptsCount" + 1, so
        // parallel wrong guesses cannot lose each other's increments.
        return _context.PasswordResetCodes
            .Where(x => x.Id == codeId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.AttemptsCount, x => x.AttemptsCount + 1), ct);
    }

    public async Task<List<PasswordResetCode>> GetAllActiveByUserIdAsync(
        int userId,
        CancellationToken ct = default)
    {
        return await _context.PasswordResetCodes
            .Where(x => x.UserId == userId)
            .Where(x => x.UsedAt == null)
            .Where(x => x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PasswordResetCode?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        return await _context.PasswordResetCodes
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(
        PasswordResetCode code,
        CancellationToken ct = default)
    {
        await _context.PasswordResetCodes.AddAsync(code, ct);
    }

    public Task UpdateAsync(
        PasswordResetCode code,
        CancellationToken ct = default)
    {
        _context.PasswordResetCodes.Update(code);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        PasswordResetCode code,
        CancellationToken ct = default)
    {
        _context.PasswordResetCodes.Remove(code);
        return Task.CompletedTask;
    }

    public async Task DeleteAllByUserIdAsync(
        int userId,
        CancellationToken ct = default)
    {
        // ✅ Массовое удаление через ExecuteDeleteAsync (EF Core 7+)
        await _context.PasswordResetCodes
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<int> DeleteExpiredAsync(
        DateTime olderThan,
        CancellationToken ct = default)
    {
        // ✅ Удаляем коды старше указанной даты
        return await _context.PasswordResetCodes
            .Where(x => x.ExpiresAt < olderThan || x.UsedAt != null)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<bool> HasActiveCodeAsync(
        int userId,
        CancellationToken ct = default)
    {
        return await _context.PasswordResetCodes
            .AnyAsync(x => x.UserId == userId
                        && x.UsedAt == null
                        && x.ExpiresAt > DateTime.UtcNow, ct);
    }

    public async Task<int> GetRequestCountAsync(
        int userId,
        DateTime from,
        CancellationToken ct = default)
    {
        return await _context.PasswordResetCodes
            .Where(x => x.UserId == userId && x.CreatedAt >= from)
            .CountAsync(ct);
    }
}