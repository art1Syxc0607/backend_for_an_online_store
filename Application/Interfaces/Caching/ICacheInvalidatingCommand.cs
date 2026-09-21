
namespace Application.Interfaces.Caching;

/// <summary>
/// Маркер: запрос поддерживает кэширование.
/// Pipeline автоматически кэширует результат.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Уникальный ключ кэша. Должен включать все параметры,
    /// влияющие на результат.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// TTL. Если null — используется дефолт (10 минут).
    /// </summary>
    TimeSpan? CacheDuration { get; }
}
