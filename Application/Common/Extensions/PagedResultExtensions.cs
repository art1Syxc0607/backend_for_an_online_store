using Application.Common;
using AutoMapper;

namespace Application.Common.Extensions;

public static class PagedResultExtensions
{
    /// <summary>
    /// Маппит PagedResult&lt;TSource&gt; в PagedResult&lt;TDest&gt; через AutoMapper.
    /// </summary>
    public static PagedResult<TDest> Map<TSource, TDest>(
        this PagedResult<TSource> source,
        IMapper mapper)
    {
        return new PagedResult<TDest>
        {
            Items = mapper.Map<List<TDest>>(source.Items),
            TotalCount = source.TotalCount,
            PageNumber = source.PageNumber,
            PageSize = source.PageSize
        };
    }

    /// <summary>
    /// Маппит PagedResult&lt;TSource&gt; в PagedResult&lt;TDest&gt; через функцию.
    /// </summary>
    public static PagedResult<TDest> Map<TSource, TDest>(
        this PagedResult<TSource> source,
        Func<TSource, TDest> mapper)
    {
        return new PagedResult<TDest>
        {
            Items = source.Items.Select(mapper).ToList(),
            TotalCount = source.TotalCount,
            PageNumber = source.PageNumber,
            PageSize = source.PageSize
        };
    }
}