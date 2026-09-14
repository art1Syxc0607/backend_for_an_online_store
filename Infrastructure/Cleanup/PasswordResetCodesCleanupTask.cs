using Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Cleanup;

public class PasswordResetCodesCleanupTask : ICleanupTask
{
    private readonly IPasswordResetRepository _repository;
    private readonly ILogger<PasswordResetCodesCleanupTask> _logger;

    public string Name => "PasswordResetCodes";
    public TimeSpan Interval => TimeSpan.FromHours(6);

    public PasswordResetCodesCleanupTask(
        IPasswordResetRepository repository,
        ILogger<PasswordResetCodesCleanupTask> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(CancellationToken ct)
    {
        // Удаляем старше 48 часов
        var threshold = DateTime.UtcNow.AddHours(-48);
        return await _repository.DeleteExpiredAsync(threshold, ct);
    }
}
