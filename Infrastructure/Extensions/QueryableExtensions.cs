using Application.Enums;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }


    public static IQueryable<Product> ApplySorting(
   this IQueryable<Product> query,
   SortBy? sortBy, bool sortDesc = true)
    {
        return (sortBy ?? SortBy.Name) switch
        {
            SortBy.Price => sortDesc
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),

            SortBy.Name => sortDesc
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),

            SortBy.Rating => sortDesc
                ? query.OrderByDescending(p => p.Reviews.Any()
                    ? p.Reviews.Average(r => r.Rating)
                    : 0)
                : query.OrderBy(p => p.Reviews.Any()
                    ? p.Reviews.Average(r => r.Rating)
                    : 0),

            SortBy.ReviewAmount => sortDesc
                ? query.OrderByDescending(p => p.Reviews.Count)
                : query.OrderBy(p => p.Reviews.Count),

            _ => query.OrderBy(p => p.Name)
        };
    }

    public static IQueryable<T> Pagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}

