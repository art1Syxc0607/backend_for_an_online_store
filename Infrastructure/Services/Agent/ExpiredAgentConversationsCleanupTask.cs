using Application.Interfaces;
using Application.Interfaces.Agent;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Agent;

/// <summary>
/// Периодическая очистка неактивных гостевых диалогов с AI-агентом.
/// Запускается единым планировщиком <see cref="CleanupBackgroundService"/>
/// через контракт <see cref="ICleanupTask"/>.
/// </summary>
public class ExpiredAgentConversationsCleanupTask : ICleanupTask
{
    private readonly IAgentConversationRepository _repository;
    private readonly AgentOptions _options;
    private readonly ILogger<ExpiredAgentConversationsCleanupTask> _logger;

    public string Name => "ExpiredAgentConversations";

    public TimeSpan Interval => TimeSpan.FromHours(_options.CleanupIntervalHours);

    public ExpiredAgentConversationsCleanupTask(
        IAgentConversationRepository repository,
        IOptions<AgentOptions> options,
        ILogger<ExpiredAgentConversationsCleanupTask> logger)
    {
        _repository = repository;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(CancellationToken ct)
    {
        if (!_options.Enabled)
            return 0;

        var deleted = await _repository.DeleteExpiredAsync(
            _options.GuestConversationRetentionDays, ct);

        if (deleted > 0)
        {
            _logger.LogInformation(
                "🧹 Deleted {Count} inactive guest conversations (no activity for {Days} days)",
                deleted, _options.GuestConversationRetentionDays);
        }

        return deleted;
    }
}
