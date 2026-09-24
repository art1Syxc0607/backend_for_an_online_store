using Application.Common.Caching;


namespace Application.Interfaces.Caching;

/// <summary>
/// Команда, которая может предоставить данные для инвалидации ПОСЛЕ выполнения.
/// </summary>
public interface ICacheInvalidatingCommandWithCallback : ICacheInvalidatingCommand
{
    /// <summary>
    /// Вызывается Handler'ом, чтобы передать данные для инвалидации.
    /// </summary>
    void SetCacheInvalidationData(CacheInvalidationContext context);
}