using System;

namespace AzureTest.Application.Common;

/// <summary>
/// Utility helper for standardizing DateTime values to UTC across all application commands and handlers.
/// Required by PostgreSQL Npgsql which enforces DateTimeKind.Utc on timestamp with time zone columns.
/// </summary>
public static class DateTimeExtensions
{
    public static DateTime ToUtc(this DateTime dt)
    {
        return dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
    }

    public static DateTime? ToUtc(this DateTime? dt)
    {
        return dt.HasValue ? ToUtc(dt.Value) : null;
    }
}
