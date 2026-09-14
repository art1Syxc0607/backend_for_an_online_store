using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;


public interface ICleanupTask
{
    /// <summary>
    /// Уникальное имя для логирования
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Через какой интервал выполнять
    /// </summary>
    TimeSpan Interval { get; }

    /// <summary>
    /// Выполнить очистку. Вернуть количество обработанных записей.
    /// </summary>
    Task<int> ExecuteAsync(CancellationToken ct);
}