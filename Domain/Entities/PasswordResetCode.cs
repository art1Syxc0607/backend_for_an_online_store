using Domain.Entities;

public class PasswordResetCode
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string CodeHash { get; private set; } = string.Empty; // ❗ Хеш, не сам код!
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public int AttemptsCount { get; private set; }
    public string? IpAddress { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsUsed => UsedAt.HasValue;
    public bool IsValid => !IsExpired && !IsUsed && AttemptsCount < MaxAttempts;

    private const int MaxAttempts = 5;
    private const int CodeLifetimeMinutes = 15;

    private PasswordResetCode() { }

    public PasswordResetCode(int userId, string codeHash, string? ipAddress = null)
    {
        UserId = userId;
        CodeHash = codeHash;
        IpAddress = ipAddress;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddMinutes(CodeLifetimeMinutes);
        AttemptsCount = 0;
    }

    public void IncrementAttempts()
    {
        AttemptsCount++;
    }

    public void MarkAsUsed()
    {
        UsedAt = DateTime.UtcNow;
    }
}