using System;
using System.Collections.Generic;
using AzureTest.Application.Habits;
using AzureTest.Application.Habits.Commands;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Enums;
using Shouldly;
using Xunit;

namespace AzureTest.UnitTest.Habits;

public class HabitTests
{
    private readonly Guid _testUserId = Guid.NewGuid();

    [Theory]
    [InlineData(HabitType.Boolean)]
    [InlineData(HabitType.Counter)]
    [InlineData(HabitType.Quantity)]
    [InlineData(HabitType.Duration)]
    [InlineData(HabitType.Rating)]
    [InlineData(HabitType.TimeOfDay)]
    [InlineData(HabitType.Checklist)]
    [InlineData(HabitType.Quit)]
    [InlineData(HabitType.Choice)]
    [InlineData(HabitType.Text)]
    public void HabitTracker_ShouldSupportAll10Types(HabitType habitType)
    {
        var habit = HabitTracker.Create(
            name: $"Test {habitType}",
            description: "Test description",
            type: habitType,
            direction: HabitDirection.AtLeast,
            target: 5.0,
            schedule: "Daily",
            frequency: HabitFrequency.Daily,
            timeZone: "UTC",
            isPrivate: true,
            typeConfig: "{\"unit\":\"ml\"}",
            userId: _testUserId
        );

        habit.Type.ShouldBe(habitType);
        habit.Direction.ShouldBe(HabitDirection.AtLeast);
        habit.Target.ShouldBe(5.0);
        habit.Goal.ShouldBe(5.0);
        habit.TimeZone.ShouldBe("UTC");
        habit.IsPrivate.ShouldBeTrue();
        habit.IsArchived.ShouldBeFalse();
        habit.TypeConfig.ShouldBe("{\"unit\":\"ml\"}");
    }

    [Fact]
    public void HabitStatsCalculator_ShouldComputeStreaksDynamically()
    {
        var habit = HabitTracker.Create(
            name: "Morning Run",
            description: "5km run",
            type: HabitType.Boolean,
            direction: HabitDirection.AtLeast,
            target: 1.0,
            schedule: "Daily",
            frequency: HabitFrequency.Daily,
            timeZone: "UTC",
            isPrivate: false,
            typeConfig: null,
            userId: _testUserId
        );

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<HabitLog>
        {
            HabitLog.Create(habit.Id, _testUserId, today, DateTime.UtcNow, "true", 1.0, null, 1.0),
            HabitLog.Create(habit.Id, _testUserId, today.AddDays(-1), DateTime.UtcNow.AddDays(-1), "true", 1.0, null, 1.0),
            HabitLog.Create(habit.Id, _testUserId, today.AddDays(-2), DateTime.UtcNow.AddDays(-2), "true", 1.0, null, 1.0)
        };

        var stats = HabitStatsCalculator.Calculate(habit, logs, today);

        stats.CurrentStreak.ShouldBe(3);
        stats.LongestStreak.ShouldBe(3);
        stats.IsCompletedToday.ShouldBeTrue();
    }

    [Fact]
    public void HabitStatsCalculator_ShouldUseTargetSnapshot_NotCurrentTarget()
    {
        // Habit previously had target 3, user logged 3, then habit target changed to 5!
        var habit = HabitTracker.Create(
            name: "Water intake",
            description: "Drink glasses of water",
            type: HabitType.Quantity,
            direction: HabitDirection.AtLeast,
            target: 5.0, // New target!
            schedule: "Daily",
            frequency: HabitFrequency.Daily,
            timeZone: "UTC",
            isPrivate: false,
            typeConfig: null,
            userId: _testUserId
        );

        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-3);
        // Log snapshot was 3.0 at the time of logging
        var log = HabitLog.Create(habit.Id, _testUserId, pastDate, DateTime.UtcNow.AddDays(-3), "3", 3.0, null, targetSnapshot: 3.0);

        var summary = HabitStatsCalculator.EvaluateDay(habit, pastDate, new List<HabitLog> { log });

        // Since snapshot was 3.0 and value was 3.0, it should be marked COMPLETED even though current target is 5.0!
        summary.IsCompleted.ShouldBeTrue();
        summary.Target.ShouldBe(3.0);
    }

    [Fact]
    public void Counter_ShouldAggregateMultipleClicksPerDay()
    {
        var habit = HabitTracker.Create(
            name: "Glasses of water",
            description: "+1 per glass",
            type: HabitType.Counter,
            direction: HabitDirection.AtLeast,
            target: 3.0,
            schedule: "Daily",
            frequency: HabitFrequency.Daily,
            timeZone: "UTC",
            isPrivate: false,
            typeConfig: null,
            userId: _testUserId
        );

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var logs = new List<HabitLog>
        {
            HabitLog.Create(habit.Id, _testUserId, today, DateTime.UtcNow.AddHours(-3), "1", 1.0, null, 3.0),
            HabitLog.Create(habit.Id, _testUserId, today, DateTime.UtcNow.AddHours(-2), "1", 1.0, null, 3.0),
            HabitLog.Create(habit.Id, _testUserId, today, DateTime.UtcNow.AddHours(-1), "1", 1.0, null, 3.0)
        };

        var stats = HabitStatsCalculator.Calculate(habit, logs, today);

        stats.TodayNumericValue.ShouldBe(3.0);
        stats.IsCompletedToday.ShouldBeTrue();
    }

    [Fact]
    public void QuitHabit_ShouldComputeDaysClean()
    {
        var habit = HabitTracker.Create(
            name: "Quit Smoking",
            description: "Abstinence",
            type: HabitType.Quit,
            direction: HabitDirection.AtLeast,
            target: 0,
            schedule: "Daily",
            frequency: HabitFrequency.Daily,
            timeZone: "UTC",
            isPrivate: false,
            typeConfig: null,
            userId: _testUserId
        );
        habit.CreatedOn = DateTime.UtcNow.AddDays(-10);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        // Relapse happened 3 days ago
        var logs = new List<HabitLog>
        {
            HabitLog.Create(habit.Id, _testUserId, today.AddDays(-3), DateTime.UtcNow.AddDays(-3), "relapse", 1.0, "slip", null)
        };

        var stats = HabitStatsCalculator.Calculate(habit, logs, today);

        stats.CurrentStreak.ShouldBe(3); // 3 days clean since relapse
        stats.IsCompletedToday.ShouldBeTrue(); // No relapse today
    }

    [Fact]
    public void CreateHabitCommandValidator_ShouldValidateRequiredFields()
    {
        var validator = new CreateHabitCommandValidator();

        var invalidCommand = new CreateHabitCommand(
            Name: "", // Empty name
            Description: null,
            Type: HabitType.Boolean,
            Direction: HabitDirection.AtLeast,
            Target: -1, // Negative target
            Schedule: null,
            Frequency: HabitFrequency.Daily,
            TimeZone: null,
            IsPrivate: false,
            TypeConfig: null
        );

        var result = validator.Validate(invalidCommand);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateHabitCommand.Name));
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateHabitCommand.Target));
    }
}
