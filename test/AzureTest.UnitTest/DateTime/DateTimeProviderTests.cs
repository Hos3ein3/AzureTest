using System;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Infrastructure.Services;
using Shouldly;
using Xunit;

namespace AzureTest.UnitTest.DateTimeTests;

public class DateTimeProviderTests
{
    private readonly IDateTimeProvider _provider = new DateTimeProvider();

    [Fact]
    public void CurrentProperties_ShouldReturnValidValues()
    {
        _provider.UtcNow.ShouldBeGreaterThan(DateTime.MinValue);
        _provider.Now.ShouldBeGreaterThan(DateTime.MinValue);
        _provider.OffsetUtcNow.ShouldBeGreaterThan(DateTimeOffset.MinValue);
        _provider.OffsetNow.ShouldBeGreaterThan(DateTimeOffset.MinValue);
        _provider.UtcToday.ShouldBeGreaterThan(DateOnly.MinValue);
        _provider.Today.ShouldBeGreaterThan(DateOnly.MinValue);
    }

    [Fact]
    public void TimeZoneConversion_ShouldConvertAccurately()
    {
        // 2026-06-01 12:00:00 UTC
        var utc = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var convertedUtc = _provider.ConvertToUtc(utc);
        convertedUtc.ShouldBe(utc);

        var offset = _provider.GetTimeZoneOffset("UTC", utc);
        offset.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void CombineMethods_ShouldWorkProperly()
    {
        var date = new DateOnly(2026, 10, 20);
        var time = new TimeOnly(15, 30, 45);

        var dt = _provider.Combine(date, time, DateTimeKind.Utc);
        dt.Year.ShouldBe(2026);
        dt.Month.ShouldBe(10);
        dt.Day.ShouldBe(20);
        dt.Hour.ShouldBe(15);
        dt.Minute.ShouldBe(30);
        dt.Second.ShouldBe(45);
        dt.Kind.ShouldBe(DateTimeKind.Utc);

        var dto = _provider.Combine(date, time, TimeSpan.FromHours(3.5));
        dto.Offset.ShouldBe(TimeSpan.FromHours(3.5));

        _provider.ToDateOnly(dt).ShouldBe(date);
        _provider.ToTimeOnly(dt).ShouldBe(time);
        _provider.ToTimeSpan(time).ShouldBe(new TimeSpan(15, 30, 45));
    }

    [Fact]
    public void ShamsiConversion_ShouldConvertNowruzAccurately()
    {
        // 2024-03-20 was 1403/01/01 (Nowruz)
        var gregorianNowruz = new DateTime(2024, 3, 20, 0, 0, 0, DateTimeKind.Utc);
        var shamsi = _provider.ToShamsi(gregorianNowruz);

        shamsi.Year.ShouldBe(1403);
        shamsi.Month.ShouldBe(1);
        shamsi.Day.ShouldBe(1);
        shamsi.MonthName.ShouldBe("Farvardin");
        shamsi.MonthNameFarsi.ShouldBe("فروردین");
        shamsi.FormattedDate.ShouldBe("1403/01/01");

        // Convert back
        var convertedBack = _provider.FromShamsi(1403, 1, 1);
        convertedBack.Date.ShouldBe(new DateTime(2024, 3, 20));
    }

    [Fact]
    public void MultiCalendarConverter_ShouldReturnAllThreeCalendars()
    {
        var dt = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);
        var set = _provider.ConvertDateTime(dt);

        set.Miladi.Year.ShouldBe(2026);
        set.Miladi.Month.ShouldBe(10);
        set.Miladi.Day.ShouldBe(20);

        set.Shamsi.Year.ShouldBe(1405);
        set.Shamsi.Month.ShouldBe(7);
        set.Shamsi.Day.ShouldBe(28);

        set.Ghamari.Year.ShouldBeGreaterThan(1440);
        set.Ghamari.Month.ShouldBeInRange(1, 12);
        set.Ghamari.Day.ShouldBeInRange(1, 30);
    }

    [Fact]
    public void ExactAgeCalculation_ShouldComputeYearsMonthsDaysHoursMinutes()
    {
        var birth = new DateTime(2000, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var asOf = new DateTime(2025, 4, 20, 15, 45, 0, DateTimeKind.Utc);

        var age = _provider.CalculateExactAge(birth, asOf);

        age.Years.ShouldBe(25);
        age.Months.ShouldBe(3);
        age.Days.ShouldBe(5);
        age.Hours.ShouldBe(5);
        age.Minutes.ShouldBe(15);
        age.TotalDays.ShouldBeGreaterThan(9000);
        age.FormattedAge.ShouldContain("25 years, 3 months, 5 days");
        age.FormattedAgeFarsi.ShouldContain("25 سال");
        age.DaysUntilNextBirthday.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ExactAgeFromShamsiCalendar_ShouldCalculateAccurately()
    {
        var asOf = new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc);
        // Birth in Shamsi: 1370/07/01 (~1991-09-23)
        var age = _provider.CalculateExactAgeFromCalendar(1370, 7, 1, 8, 0, CalendarType.Shamsi, asOf);

        age.Years.ShouldBe(35);
        age.Months.ShouldBe(0);
        age.TotalDays.ShouldBeGreaterThan(12000);
        age.ZodiacSign.ShouldContain("Libra");
    }
}
