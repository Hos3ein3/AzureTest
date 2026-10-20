using System;
using System.ComponentModel;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

public class DateTimeConverterViewModel
{
    public CalendarType SelectedCalendar { get; set; } = CalendarType.Shamsi;
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public int Second { get; set; }
    public ConvertedDateSet ConvertedResult { get; set; } = new();

    // Exact Age Section
    public CalendarType BirthCalendar { get; set; } = CalendarType.Shamsi;
    public int BirthYear { get; set; }
    public int BirthMonth { get; set; }
    public int BirthDay { get; set; }
    public int BirthHour { get; set; }
    public int BirthMinute { get; set; }
    public ExactAgeResult? AgeResult { get; set; }
}

public class ConvertApiRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public int Second { get; set; }
    public CalendarType Calendar { get; set; } = CalendarType.Shamsi;
}

public class AgeApiRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public CalendarType Calendar { get; set; } = CalendarType.Shamsi;
}

[Authorize]
[DisplayName("DateTime Converter")]
[Description("Tool for multi-calendar conversions (Shamsi, Miladi, Ghamari) and exact age calculation.")]
public class DateTimeConverterController : Controller
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public DateTimeConverterController(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    [HttpGet]
    [DisplayName("Index")]
    [Description("View and interact with DateTime and Exact Age converters.")]
    public IActionResult Index(
        CalendarType calendar = CalendarType.Shamsi,
        int? year = null,
        int? month = null,
        int? day = null,
        int hour = 0,
        int minute = 0,
        int second = 0,
        CalendarType birthCalendar = CalendarType.Shamsi,
        int? birthYear = null,
        int? birthMonth = null,
        int? birthDay = null,
        int birthHour = 0,
        int birthMinute = 0)
    {
        var nowUtc = _dateTimeProvider.UtcNow;
        var todayConverted = _dateTimeProvider.ConvertDateTime(nowUtc);

        int targetYear = year ?? (calendar == CalendarType.Shamsi ? todayConverted.Shamsi.Year : (calendar == CalendarType.Ghamari ? todayConverted.Ghamari.Year : todayConverted.Miladi.Year));
        int targetMonth = month ?? (calendar == CalendarType.Shamsi ? todayConverted.Shamsi.Month : (calendar == CalendarType.Ghamari ? todayConverted.Ghamari.Month : todayConverted.Miladi.Month));
        int targetDay = day ?? (calendar == CalendarType.Shamsi ? todayConverted.Shamsi.Day : (calendar == CalendarType.Ghamari ? todayConverted.Ghamari.Day : todayConverted.Miladi.Day));

        ConvertedDateSet convertedResult;
        try
        {
            convertedResult = _dateTimeProvider.ConvertDate(targetYear, targetMonth, targetDay, hour, minute, second, calendar);
        }
        catch
        {
            convertedResult = todayConverted;
        }

        ExactAgeResult? ageResult = null;
        if (birthYear.HasValue && birthMonth.HasValue && birthDay.HasValue)
        {
            try
            {
                ageResult = _dateTimeProvider.CalculateExactAgeFromCalendar(
                    birthYear.Value,
                    birthMonth.Value,
                    birthDay.Value,
                    birthHour,
                    birthMinute,
                    birthCalendar,
                    nowUtc);
            }
            catch
            {
                // Ignore parsing errors for age
            }
        }

        var vm = new DateTimeConverterViewModel
        {
            SelectedCalendar = calendar,
            Year = targetYear,
            Month = targetMonth,
            Day = targetDay,
            Hour = hour,
            Minute = minute,
            Second = second,
            ConvertedResult = convertedResult,
            BirthCalendar = birthCalendar,
            BirthYear = birthYear ?? (birthCalendar == CalendarType.Shamsi ? 1375 : 1996),
            BirthMonth = birthMonth ?? 1,
            BirthDay = birthDay ?? 1,
            BirthHour = birthHour,
            BirthMinute = birthMinute,
            AgeResult = ageResult
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConvertDate(DateTimeConverterViewModel model)
    {
        return RedirectToAction(nameof(Index), new
        {
            calendar = model.SelectedCalendar,
            year = model.Year,
            month = model.Month,
            day = model.Day,
            hour = model.Hour,
            minute = model.Minute,
            second = model.Second,
            birthCalendar = model.BirthCalendar,
            birthYear = model.BirthYear > 0 ? (int?)model.BirthYear : null,
            birthMonth = model.BirthMonth > 0 ? (int?)model.BirthMonth : null,
            birthDay = model.BirthDay > 0 ? (int?)model.BirthDay : null,
            birthHour = model.BirthHour,
            birthMinute = model.BirthMinute
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CalculateAge(DateTimeConverterViewModel model)
    {
        return RedirectToAction(nameof(Index), new
        {
            calendar = model.SelectedCalendar,
            year = model.Year,
            month = model.Month,
            day = model.Day,
            hour = model.Hour,
            minute = model.Minute,
            second = model.Second,
            birthCalendar = model.BirthCalendar,
            birthYear = model.BirthYear,
            birthMonth = model.BirthMonth,
            birthDay = model.BirthDay,
            birthHour = model.BirthHour,
            birthMinute = model.BirthMinute
        });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    [Route("api/datetime/convert")]
    public IActionResult ApiConvert([FromBody] ConvertApiRequest req)
    {
        try
        {
            var result = _dateTimeProvider.ConvertDate(req.Year, req.Month, req.Day, req.Hour, req.Minute, req.Second, req.Calendar);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    [Route("api/datetime/age")]
    public IActionResult ApiCalculateAge([FromBody] AgeApiRequest req)
    {
        try
        {
            var result = _dateTimeProvider.CalculateExactAgeFromCalendar(req.Year, req.Month, req.Day, req.Hour, req.Minute, req.Calendar);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
