namespace Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task BeginTransactionAsync(CancellationToken ct = default);

    Task CommitTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Отцепляет все отслеживаемые сущности от контекста.
    /// Не откатывает базу данных: вызывать только между транзакциями,
    /// когда несохранённые изменения больше не нужны.
    /// </summary>
    void DiscardPendingChanges();

    Task RollbackTransactionAsync(CancellationToken ct = default);
}
