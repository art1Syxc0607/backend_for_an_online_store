using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IFavoriteRepository
{
    Task<bool> IsFavoritedAsync(int userId, int productId, CancellationToken ct = default);
    Task<List<Favorite>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task AddAsync(Favorite favorite, CancellationToken ct = default);
    /// <summary>
    /// Удаляет запись из избранного. Возвращает количество удалённых строк
    /// (0, если записи уже нет) — операция идемпотентна.
    /// </summary>
    Task<int> RemoveAsync(int userId, int productId, CancellationToken ct = default);
}
