using System.Linq.Expressions;
using Laraue.Apps.RealEstate.Contracts;

namespace Laraue.Apps.RealEstate.AppServices.Services;

public static class QueryableExtensions
{
    public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TKey>> keySelector,
        SortOrder sortOrder)
    {
        return sortOrder == SortOrder.Ascending
            ? source.OrderBy(keySelector)
            : source.OrderByDescending(keySelector);
    }
}