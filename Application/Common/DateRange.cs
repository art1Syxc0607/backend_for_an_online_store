namespace Application.Common;

/// <summary>
/// Проверки периодов. Сравниваются только календарные дни, без времени:
/// репозитории приводят границы периода к <see cref="DateTime.Date"/>.
/// </summary>
public static class DateRange
{
    /// <summary>
    /// Проверяет, что первая граница периода не позже последней.
    /// Если хотя бы одна граница не задана, период считается корректным.
    /// </summary>
    public static bool IsValid(DateTime? firstDay, DateTime? lastDay)
        => !firstDay.HasValue
           || !lastDay.HasValue
           || firstDay.Value.Date <= lastDay.Value.Date;
}
