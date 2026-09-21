using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Caching;

/// <summary>
/// Маркер: команда инвалидирует указанные префиксы кэша
/// после успешного выполнения.
/// </summary>
public interface ICacheInvalidatingCommand
{
    /// <summary>
    /// Префиксы ключей для инвалидации.
    /// Например: "products:", "products:popular:"
    /// </summary>
    IEnumerable<string> CachePrefixesToInvalidate { get; }
}
