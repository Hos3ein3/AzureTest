using System;
using System.Globalization;
using AzureTest.Application.Interfaces.Services;

namespace AzureTest.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    private static readonly PersianCalendar _persianCalendar = new();
    private static readonly HijriCalendar _hijriCalendar = new();
    private static readonly GregorianCalendar _gregorianCalendar = new();

    private static readonly string[] ShamsiMonthNamesEn =
    {
        "Farvardin", "Ordibehesht", "Khordad", "Tir", "Mordad", "Shahrivar",
        "Mehr", "Aban", "Azar", "Dey", "Bahman", "Esfand"
    };

    private static readonly string[] ShamsiMonthNamesFa =
    {
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    };

    private static readonly string[] GhamariMonthNamesEn =
    {
        "Muharram", "Safar", "Rabi' al-Awwal", "Rabi' al-Thani",
        "Jumada al-Awwal", "Jumada al-Thani", "Rajab", "Sha'ban",
        "Ramadan", "Shawwal", "Dhu al-Qi'dah", "Dhu al-Hijjah"
    };

    private static readonly string[] GhamariMonthNamesFa =
    {
        "محرم", "صفر", "ربیع‌الاول", "ربیع‌الثانی",
        "جمادی‌الاول", "جمادی‌الثانی", "رجب", "شعبان",
        "رمضان", "شوال", "ذی‌القعده", "ذی‌الحجه"
    };

    private static readonly string[] MiladiMonthNamesEn =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    private static readonly string[] MiladiMonthNamesFa =
    {
        "ژانویه", "فوریه", "مارس", "آوریل", "مه", "ژوئن",
        "ژوئیه", "اوت", "سپتامبر", "اکتبر", "نوامبر", "دسامبر"
    };

    private static readonly (string En, string Fa)[] DaysOfWeekNames =
    {
        ("Sunday", "یکشنبه"),
        ("Monday", "دوشنبه"),
        ("Tuesday", "سه‌شنبه"),
        ("Wednesday", "چهارشنبه"),
        ("Thursday", "پنج‌شنبه"),
        ("Friday", "جمعه"),
        ("Saturday", "شنبه")
    };

    private static readonly (string En, string Fa)[] ZodiacSigns =
    {
        ("Aries (The Ram)", "حمل (فروردین) ♈"),
        ("Taurus (The Bull)", "ثور (اردیبهشت) ♉"),
        ("Gemini (The Twins)", "جوزا (خرداد) ♊"),
        ("Cancer (The Crab)", "سرطان (تیر) ♋"),
        ("Leo (The Lion)", "اسد (مرداد) ♌"),
        ("Virgo (The Virgin)", "سنبله (شهریور) ♍"),
        ("Libra (The Scales)", "میزان (مهر) ♎"),
        ("Scorpio (The Scorpion)", "عقرب (آبان) ♏"),
        ("Sagittarius (The Archer)", "قوس (آذر) ♐"),
        ("Capricorn (The Goat)", "جدی (دی) ♑"),
        ("Aquarius (The Water Bearer)", "دلو (بهمن) ♒"),
        ("Pisces (The Fish)", "حوت (اسفند) ♓")
    };

    // ── Current Time Properties ──────────────────────────────────────────
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    public DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow;
    public DateTimeOffset OffsetNow => DateTimeOffset.Now;
    public DateOnly UtcToday => DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
    public TimeOnly UtcTime => TimeOnly.FromDateTime(DateTime.UtcNow);
    public TimeOnly CurrentTime => TimeOnly.FromDateTime(DateTime.Now);

    // ── TimeZone-Aware Now ──────────────────────────────────────────────
    public DateTime NowInTimeZone(string timeZoneId)
    {
        var tz = ResolveTimeZone(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
    }

    public DateTimeOffset OffsetNowInTimeZone(string timeZoneId)
    {
        var tz = ResolveTimeZone(timeZoneId);
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
    }

    public DateOnly TodayInTimeZone(string timeZoneId)
    {
        return DateOnly.FromDateTime(NowInTimeZone(timeZoneId));
    }

    public TimeOnly TimeInTimeZone(string timeZoneId)
    {
        return TimeOnly.FromDateTime(NowInTimeZone(timeZoneId));
    }

    // ── UTC & TimeZone Conversions ──────────────────────────────────────
    public DateTime ConvertToUtc(DateTime dateTime, string? sourceTimeZoneId = null)
    {
        if (dateTime.Kind == DateTimeKind.Utc) return dateTime;

        if (string.IsNullOrWhiteSpace(sourceTimeZoneId))
        {
            return dateTime.Kind == DateTimeKind.Local
                ? dateTime.ToUniversalTime()
                : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        var sourceTz = ResolveTimeZone(sourceTimeZoneId);
        var unspecified = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, sourceTz);
    }

    public DateTime ConvertFromUtc(DateTime utcDateTime, string targetTimeZoneId)
    {
        var tz = ResolveTimeZone(targetTimeZoneId);
        var utc = utcDateTime.Kind == DateTimeKind.Utc ? utcDateTime : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
    }

    public DateTime ConvertTimeZone(DateTime dateTime, string sourceTimeZoneId, string targetTimeZoneId)
    {
        var sourceTz = ResolveTimeZone(sourceTimeZoneId);
        var targetTz = ResolveTimeZone(targetTimeZoneId);
        var unspecified = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTime(unspecified, sourceTz, targetTz);
    }

    public DateTimeOffset ConvertToUtc(DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToUniversalTime();
    }

    public DateTimeOffset ConvertFromUtc(DateTimeOffset utcDateTimeOffset, string targetTimeZoneId)
    {
        var tz = ResolveTimeZone(targetTimeZoneId);
        return TimeZoneInfo.ConvertTime(utcDateTimeOffset.ToUniversalTime(), tz);
    }

    public DateTimeOffset ConvertTimeZone(DateTimeOffset dateTimeOffset, string targetTimeZoneId)
    {
        var tz = ResolveTimeZone(targetTimeZoneId);
        return TimeZoneInfo.ConvertTime(dateTimeOffset, tz);
    }

    public TimeOnly ConvertTime(TimeOnly time, string sourceTimeZoneId, string targetTimeZoneId, DateOnly? referenceDate = null)
    {
        var refDate = referenceDate ?? UtcToday;
        var dt = Combine(refDate, time, DateTimeKind.Unspecified);
        var convertedDt = ConvertTimeZone(dt, sourceTimeZoneId, targetTimeZoneId);
        return TimeOnly.FromDateTime(convertedDt);
    }

    public DateOnly ConvertDate(DateOnly date, string sourceTimeZoneId, string targetTimeZoneId, TimeOnly? referenceTime = null)
    {
        var refTime = referenceTime ?? new TimeOnly(12, 0);
        var dt = Combine(date, refTime, DateTimeKind.Unspecified);
        var convertedDt = ConvertTimeZone(dt, sourceTimeZoneId, targetTimeZoneId);
        return DateOnly.FromDateTime(convertedDt);
    }

    public TimeSpan GetTimeZoneOffset(string timeZoneId, DateTime? utcDateTime = null)
    {
        var tz = ResolveTimeZone(timeZoneId);
        var utc = utcDateTime ?? DateTime.UtcNow;
        return tz.GetUtcOffset(utc);
    }

    // ── Combination & Date/Time Type Conversions ─────────────────────────
    public DateTime Combine(DateOnly date, TimeOnly time, DateTimeKind kind = DateTimeKind.Utc)
    {
        return DateTime.SpecifyKind(date.ToDateTime(time), kind);
    }

    public DateTimeOffset Combine(DateOnly date, TimeOnly time, TimeSpan offset)
    {
        return new DateTimeOffset(date.ToDateTime(time), offset);
    }

    public DateTimeOffset Combine(DateOnly date, TimeOnly time, string timeZoneId)
    {
        var tz = ResolveTimeZone(timeZoneId);
        var dt = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        var offset = tz.GetUtcOffset(dt);
        return new DateTimeOffset(dt, offset);
    }

    public DateOnly ToDateOnly(DateTime dateTime) => DateOnly.FromDateTime(dateTime);
    public DateOnly ToDateOnly(DateTimeOffset dateTimeOffset) => DateOnly.FromDateTime(dateTimeOffset.DateTime);
    public TimeOnly ToTimeOnly(DateTime dateTime) => TimeOnly.FromDateTime(dateTime);
    public TimeOnly ToTimeOnly(DateTimeOffset dateTimeOffset) => TimeOnly.FromDateTime(dateTimeOffset.DateTime);
    public TimeSpan ToTimeSpan(TimeOnly timeOnly) => timeOnly.ToTimeSpan();
    public TimeOnly FromTimeSpan(TimeSpan timeSpan) => TimeOnly.FromTimeSpan(timeSpan);

    public TimeSpan CalculateElapsed(DateTime startUtc, DateTime? endUtc = null)
    {
        var end = endUtc ?? DateTime.UtcNow;
        return end - startUtc;
    }

    public TimeSpan CalculateElapsed(DateTimeOffset start, DateTimeOffset? end = null)
    {
        var endOffset = end ?? DateTimeOffset.UtcNow;
        return endOffset - start;
    }

    // ── Calendar Conversions ─────────────────────────────────────────────
    public ConvertedDateSet ConvertDate(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, CalendarType sourceCalendar = CalendarType.Miladi)
    {
        DateTime gregorian;

        switch (sourceCalendar)
        {
            case CalendarType.Shamsi:
                gregorian = FromShamsi(year, month, day, hour, minute, second);
                break;
            case CalendarType.Ghamari:
                gregorian = FromGhamari(year, month, day, hour, minute, second);
                break;
            case CalendarType.Miladi:
            default:
                gregorian = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
                break;
        }

        return ConvertDateTime(gregorian);
    }

    public ConvertedDateSet ConvertDateTime(DateTime dateTime)
    {
        var utc = dateTime.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc) : dateTime;

        return new ConvertedDateSet
        {
            Shamsi = ToShamsi(utc),
            Miladi = ToMiladi(utc),
            Ghamari = ToGhamari(utc),
            GregorianDateTime = utc,
            GregorianDateTimeOffset = new DateTimeOffset(utc)
        };
    }

    public CalendarDateResult ToShamsi(DateTime dateTime)
    {
        var dt = ClampForCalendar(dateTime, _persianCalendar);
        int year = _persianCalendar.GetYear(dt);
        int month = _persianCalendar.GetMonth(dt);
        int day = _persianCalendar.GetDayOfMonth(dt);
        int hour = dt.Hour;
        int minute = dt.Minute;
        int second = dt.Second;
        var dow = dt.DayOfWeek;

        int monthIndex = Math.Clamp(month - 1, 0, 11);
        int dowIndex = (int)dow;

        bool isLeap = _persianCalendar.IsLeapYear(year);
        int dayOfYear = _persianCalendar.GetDayOfYear(dt);

        return new CalendarDateResult
        {
            Calendar = CalendarType.Shamsi,
            Year = year,
            Month = month,
            Day = day,
            Hour = hour,
            Minute = minute,
            Second = second,
            MonthName = ShamsiMonthNamesEn[monthIndex],
            MonthNameFarsi = ShamsiMonthNamesFa[monthIndex],
            DayOfWeekName = DaysOfWeekNames[dowIndex].En,
            DayOfWeekNameFarsi = DaysOfWeekNames[dowIndex].Fa,
            FormattedDate = $"{year:D4}/{month:D2}/{day:D2}",
            FormattedDateTime = $"{year:D4}/{month:D2}/{day:D2} {hour:D2}:{minute:D2}:{second:D2}",
            FullText = $"{DaysOfWeekNames[dowIndex].Fa}، {day} {ShamsiMonthNamesFa[monthIndex]} {year}",
            IsLeapYear = isLeap,
            DayOfYear = dayOfYear
        };
    }

    public CalendarDateResult ToMiladi(DateTime dateTime)
    {
        var dt = dateTime;
        int year = dt.Year;
        int month = dt.Month;
        int day = dt.Day;
        int hour = dt.Hour;
        int minute = dt.Minute;
        int second = dt.Second;
        var dow = dt.DayOfWeek;

        int monthIndex = Math.Clamp(month - 1, 0, 11);
        int dowIndex = (int)dow;

        bool isLeap = DateTime.IsLeapYear(year);

        return new CalendarDateResult
        {
            Calendar = CalendarType.Miladi,
            Year = year,
            Month = month,
            Day = day,
            Hour = hour,
            Minute = minute,
            Second = second,
            MonthName = MiladiMonthNamesEn[monthIndex],
            MonthNameFarsi = MiladiMonthNamesFa[monthIndex],
            DayOfWeekName = DaysOfWeekNames[dowIndex].En,
            DayOfWeekNameFarsi = DaysOfWeekNames[dowIndex].Fa,
            FormattedDate = $"{year:D4}-{month:D2}-{day:D2}",
            FormattedDateTime = $"{year:D4}-{month:D2}-{day:D2} {hour:D2}:{minute:D2}:{second:D2}",
            FullText = $"{DaysOfWeekNames[dowIndex].En}, {month:D2}/{day:D2}/{year:D4}",
            IsLeapYear = isLeap,
            DayOfYear = dt.DayOfYear
        };
    }

    public CalendarDateResult ToGhamari(DateTime dateTime)
    {
        var dt = ClampForCalendar(dateTime, _hijriCalendar);
        int year = _hijriCalendar.GetYear(dt);
        int month = _hijriCalendar.GetMonth(dt);
        int day = _hijriCalendar.GetDayOfMonth(dt);
        int hour = dt.Hour;
        int minute = dt.Minute;
        int second = dt.Second;
        var dow = dt.DayOfWeek;

        int monthIndex = Math.Clamp(month - 1, 0, 11);
        int dowIndex = (int)dow;

        bool isLeap = _hijriCalendar.IsLeapYear(year);
        int dayOfYear = _hijriCalendar.GetDayOfYear(dt);

        return new CalendarDateResult
        {
            Calendar = CalendarType.Ghamari,
            Year = year,
            Month = month,
            Day = day,
            Hour = hour,
            Minute = minute,
            Second = second,
            MonthName = GhamariMonthNamesEn[monthIndex],
            MonthNameFarsi = GhamariMonthNamesFa[monthIndex],
            DayOfWeekName = DaysOfWeekNames[dowIndex].En,
            DayOfWeekNameFarsi = DaysOfWeekNames[dowIndex].Fa,
            FormattedDate = $"{year:D4}/{month:D2}/{day:D2}",
            FormattedDateTime = $"{year:D4}/{month:D2}/{day:D2} {hour:D2}:{minute:D2}:{second:D2}",
            FullText = $"{DaysOfWeekNames[dowIndex].Fa}، {day} {GhamariMonthNamesFa[monthIndex]} {year}",
            IsLeapYear = isLeap,
            DayOfYear = dayOfYear
        };
    }

    public DateTime FromShamsi(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
    {
        month = Math.Clamp(month, 1, 12);
        day = Math.Clamp(day, 1, 31);
        return _persianCalendar.ToDateTime(year, month, day, hour, minute, second, 0);
    }

    public DateTime FromGhamari(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
    {
        month = Math.Clamp(month, 1, 12);
        day = Math.Clamp(day, 1, 30);
        return _hijriCalendar.ToDateTime(year, month, day, hour, minute, second, 0);
    }

    // ── Exact Age Calculator ─────────────────────────────────────────────
    public ExactAgeResult CalculateExactAge(DateTime birthDateTime, DateTime? asOfDateTime = null)
    {
        var asOf = asOfDateTime ?? DateTime.UtcNow;

        if (birthDateTime > asOf)
        {
            // Future birthdate -> 0 age
            return new ExactAgeResult
            {
                FormattedAge = "Not born yet",
                FormattedAgeFarsi = "هنوز متولد نشده است"
            };
        }

        int years = asOf.Year - birthDateTime.Year;
        int months = asOf.Month - birthDateTime.Month;
        int days = asOf.Day - birthDateTime.Day;
        int hours = asOf.Hour - birthDateTime.Hour;
        int minutes = asOf.Minute - birthDateTime.Minute;
        int seconds = asOf.Second - birthDateTime.Second;

        if (seconds < 0)
        {
            seconds += 60;
            minutes--;
        }
        if (minutes < 0)
        {
            minutes += 60;
            hours--;
        }
        if (hours < 0)
        {
            hours += 24;
            days--;
        }
        if (days < 0)
        {
            var prevMonth = asOf.AddMonths(-1);
            days += DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            months--;
        }
        if (months < 0)
        {
            months += 12;
            years--;
        }

        TimeSpan totalElapsed = asOf - birthDateTime;
        long totalDays = (long)totalElapsed.TotalDays;
        long totalHours = (long)totalElapsed.TotalHours;
        long totalMinutes = (long)totalElapsed.TotalMinutes;
        long totalSeconds = (long)totalElapsed.TotalSeconds;

        // Next birthday calculation
        DateTime nextBirthday;
        try
        {
            nextBirthday = new DateTime(asOf.Year, birthDateTime.Month, Math.Min(birthDateTime.Day, DateTime.DaysInMonth(asOf.Year, birthDateTime.Month)));
            if (nextBirthday < asOf.Date)
            {
                nextBirthday = new DateTime(asOf.Year + 1, birthDateTime.Month, Math.Min(birthDateTime.Day, DateTime.DaysInMonth(asOf.Year + 1, birthDateTime.Month)));
            }
        }
        catch
        {
            nextBirthday = asOf.AddYears(1);
        }

        int daysUntilNextBirthday = Math.Max(0, (nextBirthday.Date - asOf.Date).Days);
        var nextBdayDow = nextBirthday.DayOfWeek;
        int dowIndex = (int)nextBdayDow;

        // Zodiac determination from Shamsi month
        var shamsiResult = ToShamsi(birthDateTime);
        int zIndex = Math.Clamp(shamsiResult.Month - 1, 0, 11);
        var zodiac = ZodiacSigns[zIndex];

        return new ExactAgeResult
        {
            Years = years,
            Months = months,
            Days = days,
            Hours = hours,
            Minutes = minutes,
            Seconds = seconds,
            TotalDays = totalDays,
            TotalHours = totalHours,
            TotalMinutes = totalMinutes,
            TotalSeconds = totalSeconds,
            FormattedAge = $"{years} years, {months} months, {days} days, {hours} hours, {minutes} minutes",
            FormattedAgeFarsi = $"{years} سال و {months} ماه و {days} روز و {hours} ساعت و {minutes} دقیقه",
            DaysUntilNextBirthday = daysUntilNextBirthday,
            NextBirthdayDayOfWeek = DaysOfWeekNames[dowIndex].En,
            NextBirthdayDayOfWeekFarsi = DaysOfWeekNames[dowIndex].Fa,
            ZodiacSign = zodiac.En,
            ZodiacSignFarsi = zodiac.Fa
        };
    }

    public ExactAgeResult CalculateExactAgeFromCalendar(int year, int month, int day, int hour = 0, int minute = 0, CalendarType calendar = CalendarType.Shamsi, DateTime? asOfDateTime = null)
    {
        DateTime birthGregorian;
        switch (calendar)
        {
            case CalendarType.Shamsi:
                birthGregorian = FromShamsi(year, month, day, hour, minute);
                break;
            case CalendarType.Ghamari:
                birthGregorian = FromGhamari(year, month, day, hour, minute);
                break;
            case CalendarType.Miladi:
            default:
                birthGregorian = new DateTime(year, Math.Clamp(month, 1, 12), Math.Clamp(day, 1, 31), hour, minute, 0, DateTimeKind.Utc);
                break;
        }

        return CalculateExactAge(birthGregorian, asOfDateTime);
    }

    // ── Helper Methods ───────────────────────────────────────────────────
    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId) || timeZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            // Try common aliases (e.g. Iran / Tehran)
            if (timeZoneId.Contains("Iran", StringComparison.OrdinalIgnoreCase) || timeZoneId.Contains("Tehran", StringComparison.OrdinalIgnoreCase))
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"); } catch { }
                try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran"); } catch { }
            }

            return TimeZoneInfo.Utc;
        }
    }

    private static DateTime ClampForCalendar(DateTime dt, Calendar cal)
    {
        if (dt < cal.MinSupportedDateTime) return cal.MinSupportedDateTime;
        if (dt > cal.MaxSupportedDateTime) return cal.MaxSupportedDateTime;
        return dt;
    }
}
