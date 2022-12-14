namespace SalesManagementSystem.Server.Helpers;

using System.Linq.Expressions;

static class QueryableExtensions
{
    public static IQueryable<T> TakeIfNotNull<T>(
        this IQueryable<T> queryable,
        int? count)
    {
        if (count is null)
        {
            return queryable;
        }
        return queryable.Take(count.Value);
    }

    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> queryable,
        bool useWhere,
        Expression<Func<T, bool>> predicate
    ) =>
        useWhere ? queryable.Where(predicate) : queryable;
}