using System;

namespace AzureTest.Application.Interfaces.Services;

public enum CalendarType
{
    Miladi = 1,
    Shamsi = 2,
    Ghamari = 3
}

public class CalendarDateResult
{
    public CalendarType Calendar { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public int Second { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public string MonthNameFarsi { get; set; } = string.Empty;
    public string DayOfWeekName { get; set; } = string.Empty;
    public string DayOfWeekNameFarsi { get; set; } = string.Empty;
    public string FormattedDate { get; set; } = string.Empty;
    public string FormattedDateTime { get; set; } = string.Empty;
    public string FullText { get; set; } = string.Empty;
    public bool IsLeapYear { get; set; }
    public int DayOfYear { get; set; }
}

public class ConvertedDateSet
{
    public CalendarDateResult Shamsi { get; set; } = new();
    public CalendarDateResult Miladi { get; set; } = new();
    public CalendarDateResult Ghamari { get; set; } = new();
    public DateTime GregorianDateTime { get; set; }
    public DateTimeOffset GregorianDateTimeOffset { get; set; }
}

public class ExactAgeResult
{
    public int Years { get; set; }
    public int Months { get; set; }
    public int Days { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }

    public long TotalDays { get; set; }
    public long TotalHours { get; set; }
    public long TotalMinutes { get; set; }
    public long TotalSeconds { get; set; }

    public string FormattedAge { get; set; } = string.Empty;
    public string FormattedAgeFarsi { get; set; } = string.Empty;

    public int DaysUntilNextBirthday { get; set; }
    public string NextBirthdayDayOfWeek { get; set; } = string.Empty;
    public string NextBirthdayDayOfWeekFarsi { get; set; } = string.Empty;
    public string ZodiacSign { get; set; } = string.Empty;
    public string ZodiacSignFarsi { get; set; } = string.Empty;
}

public interface IDateTimeProvider
{
    // ── Current Time Properties ──────────────────────────────────────────
    DateTime UtcNow { get; }
    DateTime Now { get; }
    DateTimeOffset OffsetUtcNow { get; }
    DateTimeOffset OffsetNow { get; }
    DateOnly UtcToday { get; }
    DateOnly Today { get; }
    TimeOnly UtcTime { get; }
    TimeOnly CurrentTime { get; }

    // ── TimeZone-Aware Now ──────────────────────────────────────────────
    DateTime NowInTimeZone(string timeZoneId);
    DateTimeOffset OffsetNowInTimeZone(string timeZoneId);
    DateOnly TodayInTimeZone(string timeZoneId);
    TimeOnly TimeInTimeZone(string timeZoneId);

    // ── UTC & TimeZone Conversions ──────────────────────────────────────
    DateTime ConvertToUtc(DateTime dateTime, string? sourceTimeZoneId = null);
    DateTime ConvertFromUtc(DateTime utcDateTime, string targetTimeZoneId);
    DateTime ConvertTimeZone(DateTime dateTime, string sourceTimeZoneId, string targetTimeZoneId);

    DateTimeOffset ConvertToUtc(DateTimeOffset dateTimeOffset);
    DateTimeOffset ConvertFromUtc(DateTimeOffset utcDateTimeOffset, string targetTimeZoneId);
    DateTimeOffset ConvertTimeZone(DateTimeOffset dateTimeOffset, string targetTimeZoneId);

    TimeOnly ConvertTime(TimeOnly time, string sourceTimeZoneId, string targetTimeZoneId, DateOnly? referenceDate = null);
    DateOnly ConvertDate(DateOnly date, string sourceTimeZoneId, string targetTimeZoneId, TimeOnly? referenceTime = null);
    TimeSpan GetTimeZoneOffset(string timeZoneId, DateTime? utcDateTime = null);

    // ── Combination & Date/Time Type Conversions ─────────────────────────
    DateTime Combine(DateOnly date, TimeOnly time, DateTimeKind kind = DateTimeKind.Utc);
    DateTimeOffset Combine(DateOnly date, TimeOnly time, TimeSpan offset);
    DateTimeOffset Combine(DateOnly date, TimeOnly time, string timeZoneId);

    DateOnly ToDateOnly(DateTime dateTime);
    DateOnly ToDateOnly(DateTimeOffset dateTimeOffset);
    TimeOnly ToTimeOnly(DateTime dateTime);
    TimeOnly ToTimeOnly(DateTimeOffset dateTimeOffset);
    TimeSpan ToTimeSpan(TimeOnly timeOnly);
    TimeOnly FromTimeSpan(TimeSpan timeSpan);

    TimeSpan CalculateElapsed(DateTime startUtc, DateTime? endUtc = null);
    TimeSpan CalculateElapsed(DateTimeOffset start, DateTimeOffset? end = null);

    // ── Calendar Conversions (Miladi, Shamsi, Ghamari) ────────────────────
    ConvertedDateSet ConvertDate(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, CalendarType sourceCalendar = CalendarType.Miladi);
    ConvertedDateSet ConvertDateTime(DateTime dateTime);

    CalendarDateResult ToShamsi(DateTime dateTime);
    CalendarDateResult ToMiladi(DateTime dateTime);
    CalendarDateResult ToGhamari(DateTime dateTime);

    DateTime FromShamsi(int year, int month, int day, int hour = 0, int minute = 0, int second = 0);
    DateTime FromGhamari(int year, int month, int day, int hour = 0, int minute = 0, int second = 0);

    // ── Exact Age Calculator ─────────────────────────────────────────────
    ExactAgeResult CalculateExactAge(DateTime birthDateTime, DateTime? asOfDateTime = null);
    ExactAgeResult CalculateExactAgeFromCalendar(int year, int month, int day, int hour = 0, int minute = 0, CalendarType calendar = CalendarType.Shamsi, DateTime? asOfDateTime = null);
}
