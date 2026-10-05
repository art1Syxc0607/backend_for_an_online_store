using Application.Interfaces.Agent;
using Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Agent;

public class ExpiredAgentConversationsCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AgentOptions _options;
    private readonly ILogger<ExpiredAgentConversationsCleanupService> _logger;

    public ExpiredAgentConversationsCleanupService(
        IServiceProvider serviceProvider,
        IOptions<AgentOptions> options,  // ✅ IOptions<T>!
        ILogger<ExpiredAgentConversationsCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;  // ✅ .Value
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("🧹 Agent cleanup service is disabled");
            return;
        }

        var interval = TimeSpan.FromHours(_options.CleanupIntervalHours);

        _logger.LogInformation(
            "🧹 Agent cleanup service started. " +
            "Interval: {Interval}, Retention: {Days} days",
            interval, _options.GuestConversationRetentionDays);

        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error during agent conversations cleanup");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IAgentConversationRepository>();

        // ✅ Передаём retention days — репозиторий сам считает threshold
        var deleted = await repository.DeleteExpiredAsync(
            _options.GuestConversationRetentionDays, ct);

        if (deleted > 0)
        {
            _logger.LogInformation(
                "🧹 Deleted {Count} inactive guest conversations " +
                "(no activity for {Days} days)",
                deleted, _options.GuestConversationRetentionDays);
        }
        else
        {
            _logger.LogDebug("🧹 No inactive guest conversations");
        }
    }
}
