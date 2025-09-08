using System.Linq.Expressions;
using Laraue.Apps.RealEstate.Abstractions;

namespace Laraue.Apps.RealEstate.Db.Storage;

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