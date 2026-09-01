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
   SortProductBy? sortBy, bool sortDesc = true)
    {
        return (sortBy ?? SortProductBy.Name) switch
        {
            SortProductBy.Price => sortDesc
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),

            SortProductBy.Name => sortDesc
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),

            SortProductBy.AvailableQuantity => sortDesc
                ? query.OrderByDescending(p => p.AvailableQuantity)
                : query.OrderBy(p => p.AvailableQuantity),

            SortProductBy.Rating => sortDesc
                ? query.OrderByDescending(p => p.Reviews.Any()
                    ? p.Reviews.Average(r => r.Rating)
                    : 0)
                : query.OrderBy(p => p.Reviews.Any()
                    ? p.Reviews.Average(r => r.Rating)
                    : 0),

            SortProductBy.ReviewAmount => sortDesc
                ? query.OrderByDescending(p => p.Reviews.Count)
                : query.OrderBy(p => p.Reviews.Count),

            SortProductBy.PaymentAmount => sortDesc
                ? query.OrderByDescending(p => p.AmountOfPaid)
                : query.OrderBy(p => p.AmountOfPaid),

            _ => query.OrderBy(p => p.Name) // // Если ни одно не совпало (default)
        };
    }

    public static IQueryable<Order> ApplyOrderSorting(
       this IQueryable<Order> query,
       SortOrderBy? sortBy, bool sortDesc = true)
    {
        return (sortBy ?? SortOrderBy.DateOfCreation) switch
        {
            SortOrderBy.Status => sortDesc
                ? query.OrderByDescending(p => p.Status)
                : query.OrderBy(p => p.Status),

            SortOrderBy.UserId => sortDesc
                ? query.OrderByDescending(p => p.UserId)
                : query.OrderBy(p => p.UserId),

            SortOrderBy.DateOfCreation => sortDesc
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),

            _ => query.OrderByDescending(p => p.CreatedAt) // // Если ни одно не совпало (default)
        };
    }


    public static IQueryable<T> Pagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}

