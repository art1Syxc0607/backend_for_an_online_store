namespace Infrastructure.Options;

public class IpInfoOptions
{
    public const string SectionName = "IpInfo";

    /// <summary>
    /// API-ключ IPinfo.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Базовый URL API (по умолчанию — production).
    /// </summary>
    public string BaseUrl { get; set; } = "https://ipinfo.io";

    /// <summary>
    /// Таймаут запроса в секундах.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Включить логирование запросов.
    /// </summary>
    public bool Enabled { get; set; } = true;
}