using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

// Domain/Interfaces/IPasswordResetRepository.cs
public interface IPasswordResetRepository
{
    /// <summary>
    /// Получить последний активный код для пользователя
    /// </summary>
    Task<PasswordResetCode?> GetLatestByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Получить все активные коды для пользователя
    /// </summary>
    Task<List<PasswordResetCode>> GetAllActiveByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Найти код по ID
    /// </summary>
    Task<PasswordResetCode?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Добавить новый код
    /// </summary>
    Task AddAsync(PasswordResetCode code, CancellationToken ct = default);

    /// <summary>
    /// Обновить код
    /// </summary>
    Task UpdateAsync(PasswordResetCode code, CancellationToken ct = default);

    /// <summary>
    /// Удалить код
    /// </summary>
    Task DeleteAsync(PasswordResetCode code, CancellationToken ct = default);

    /// <summary>
    /// Удалить все коды пользователя (например, при успешной смене пароля)
    /// </summary>
    Task DeleteAllByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Удалить просроченные коды (для фоновой очистки)
    /// </summary>
    Task<int> DeleteExpiredAsync(DateTime olderThan, CancellationToken ct = default);

    /// <summary>
    /// Проверить, есть ли активный код у пользователя
    /// </summary>
    Task<bool> HasActiveCodeAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Получить количество запросов за период (для rate limiting)
    /// </summary>
    Task<int> GetRequestCountAsync(int userId, DateTime from, CancellationToken ct = default);
}
