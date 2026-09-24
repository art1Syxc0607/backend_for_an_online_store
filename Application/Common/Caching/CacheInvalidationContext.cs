// Application/Common/Caching/CacheInvalidationContext.cs
namespace Application.Common.Caching;

/// <summary>
/// Контекст инвалидации — Handler заполняет его, 
/// Behavior читает.
/// </summary>
public class CacheInvalidationContext
{
    private readonly List<string> _prefixes = new();

    /// <summary>
    /// Добавить префикс для инвалидации.
    /// </summary>
    public void AddPrefix(string prefix)
    {
        if (!string.IsNullOrWhiteSpace(prefix) && !_prefixes.Contains(prefix))
        {
            _prefixes.Add(prefix);
        }
    }

    /// <summary>
    /// Добавить несколько префиксов.
    /// </summary>
    public void AddPrefixes(params string[] prefixes)
    {
        foreach (var prefix in prefixes)
        {
            AddPrefix(prefix);
        }
    }

    /// <summary>
    /// Получить все префиксы.
    /// </summary>
    public IReadOnlyList<string> GetPrefixes() => _prefixes.AsReadOnly();

    /// <summary>
    /// Есть ли что-то для инвалидации.
    /// </summary>
    public bool HasPrefixes => _prefixes.Count > 0;
}