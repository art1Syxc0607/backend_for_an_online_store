namespace Application.Common;

public class PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }

    // ✅ Вычисляемые свойства (не хранятся, вычисляются на лету)
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling((double)TotalCount / PageSize)
        : 0;

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    // ✅ Пустой результат
    public static PagedResult<T> Empty(int pageNumber, int pageSize) => new()
    {
        Items = new List<T>(),
        TotalCount = 0,
        PageNumber = pageNumber,
        PageSize = pageSize
    };

    // ✅ Фабричный метод
    public static PagedResult<T> Create(
        List<T> items, int totalCount, int pageNumber, int pageSize) => new()
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
}