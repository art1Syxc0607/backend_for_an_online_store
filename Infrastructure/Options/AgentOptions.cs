

namespace Infrastructure.Options;

public class AgentOptions
{
    public const string SectionName = "Agent";

    /// <summary>
    /// Через сколько дней неактивности удалять гостевые диалоги.
    /// </summary>
    public int GuestConversationRetentionDays { get; set; } = 7;

    public int CleanupIntervalHours { get; set; } = 6;
    public bool Enabled { get; set; } = true;
}