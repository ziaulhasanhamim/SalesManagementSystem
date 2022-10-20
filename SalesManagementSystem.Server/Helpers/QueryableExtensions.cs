namespace SalesManagementSystem.Server.Helpers;

public static class QueryableExtensions
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
}