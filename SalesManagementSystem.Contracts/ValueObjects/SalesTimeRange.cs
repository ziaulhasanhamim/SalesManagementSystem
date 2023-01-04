namespace SalesManagementSystem.Contracts.ValueObjects;

using System.Diagnostics;
using System.Globalization;

public enum SalesTimeRange
{
    Today,
    Last7Days,
    Last30Days,
    ThisMonth,
    ThisYear,
    All
}

public static class SalesTimeRangeExtensions
{
    public static DateTime? GetDateTime(this SalesTimeRange timeRange) => timeRange switch
    {
        SalesTimeRange.Today => DateTime.Now.Date.ToUniversalTime(),
        SalesTimeRange.Last7Days => DateTime.Now.Date.AddDays(-6).ToUniversalTime(),
        SalesTimeRange.Last30Days => DateTime.Now.AddDays(-29).ToUniversalTime(),
        SalesTimeRange.ThisMonth =>
            new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0)
                .ToUniversalTime(),
        SalesTimeRange.ThisYear =>
            new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0)
                .ToUniversalTime(),
        SalesTimeRange.All => null,
        _ => throw new UnreachableException()
    };

    private static DateTime GetTodaysDateTime()
    {
        DateTime dt = DateTime.Now.Date;
        return dt.ToUniversalTime();
    }
}