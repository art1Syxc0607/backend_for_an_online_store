using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;

// Infrastructure/Services/CleanupBackgroundService.cs
public class CleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CleanupBackgroundService> _logger;

    // Проверяем каждую минуту: какой task пора запустить
    private static readonly TimeSpan SchedulerTick = TimeSpan.FromMinutes(1);

    public CleanupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🧹 Cleanup scheduler started");

        // Небольшая задержка при старте
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        // Словарь: когда последний раз запускали каждую задачу
        var lastRun = new Dictionary<string, DateTime>();

        using var timer = new PeriodicTimer(SchedulerTick);

        do
        {
            try
            {
                await RunDueTasksAsync(lastRun, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cleanup scheduler error");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));

        _logger.LogInformation("🧹 Cleanup scheduler stopped");
    }

    private async Task RunDueTasksAsync(
        Dictionary<string, DateTime> lastRun,
        CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var tasks = scope.ServiceProvider.GetServices<ICleanupTask>();
        var now = DateTime.UtcNow;

        foreach (var task in tasks)
        {
            // Пора ли запускать?
            if (lastRun.TryGetValue(task.Name, out var last) &&
                now - last < task.Interval)
            {
                continue;
            }

            try
            {
                _logger.LogDebug("🧹 Running cleanup task: {Name}", task.Name);
                var sw = Stopwatch.StartNew();

                var count = await task.ExecuteAsync(ct);

                sw.Stop();
                lastRun[task.Name] = now;

                if (count > 0)
                {
                    _logger.LogInformation(
                        "🧹 {Name}: cleaned {Count} records in {Duration}ms",
                        task.Name, count, sw.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogDebug(
                        "🧹 {Name}: nothing to clean ({Duration}ms)",
                        task.Name, sw.ElapsedMilliseconds);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // ✅ Один упавший task не ломает остальные
                _logger.LogError(ex,
                    "❌ Cleanup task '{Name}' failed", task.Name);
                lastRun[task.Name] = now; // Не зацикливаться
            }
        }
    }
}