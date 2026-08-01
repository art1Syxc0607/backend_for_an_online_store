using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;



public interface IPaymentRepository
{
    /// <summary>
    /// Получить платеж по ID
    /// </summary>
    Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Получить платеж по ID заказа
    /// </summary>
    Task<Payment?> GetByOrderIdAsync(int orderId, CancellationToken ct = default);

    /// <summary>
    /// Получить платеж по TransactionId (внутреннему идентификатору)
    /// </summary>
    Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default);

    /// <summary>
    /// Получить платеж по ExternalTransactionId (идентификатор от платежного шлюза)
    /// </summary>
    Task<Payment?> GetByExternalTransactionIdAsync(string externalTransactionId, CancellationToken ct = default);

    /// <summary>
    /// Получить все платежи пользователя
    /// </summary>
    Task<List<Payment>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Добавить новый платеж
    /// </summary>
    Task AddAsync(Payment payment, CancellationToken ct = default);

    /// <summary>
    /// Обновить платеж
    /// </summary>
    Task UpdateAsync(Payment payment, CancellationToken ct = default);

    /// <summary>
    /// Проверить, существует ли платеж для заказа
    /// </summary>
    Task<bool> ExistsByOrderIdAsync(int orderId, CancellationToken ct = default);

    /// <summary>
    /// Получить все платежи со статусом Pending (для фоновых задач)
    /// </summary>
    Task<List<Payment>> GetPendingPaymentsAsync(DateTime olderThan, CancellationToken ct = default);
}
