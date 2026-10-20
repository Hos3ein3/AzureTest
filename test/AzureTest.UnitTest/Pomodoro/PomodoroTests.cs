using System;
using AzureTest.Application.Pomodoro.Commands.CreatePomodoroSession;
using AzureTest.Contracts.DTOs.Pomodoro;
using AzureTest.Domain.Entities.Pomodoro;
using AzureTest.Domain.Enums.Pomodoro;
using Shouldly;
using Xunit;

namespace AzureTest.UnitTest.Pomodoro;

public class PomodoroTests
{
    [Fact]
    public void PomodoroSession_Create_ShouldInitializeWithCorrectValues()
    {
        var userId = Guid.NewGuid();
        var startTime = DateTime.UtcNow;

        var session = PomodoroSession.Create(
            userId: userId,
            title: "Code Review",
            category: "Development",
            sessionType: PomodoroSessionType.Work,
            plannedDurationSeconds: 1500,
            actualDurationSeconds: 1500,
            startedAtUtc: startTime,
            endedAtUtc: startTime.AddMinutes(25),
            status: PomodoroStatus.Completed,
            interruptionCount: 1,
            notes: "Reviewed PR #42",
            usedSkin: TimerSkin.Digital);

        session.UserId.ShouldBe(userId);
        session.Title.ShouldBe("Code Review");
        session.Category.ShouldBe("Development");
        session.SessionType.ShouldBe(PomodoroSessionType.Work);
        session.Status.ShouldBe(PomodoroStatus.Completed);
        session.PlannedDurationSeconds.ShouldBe(1500);
        session.ActualDurationSeconds.ShouldBe(1500);
        session.InterruptionCount.ShouldBe(1);
        session.Notes.ShouldBe("Reviewed PR #42");
        session.UsedSkin.ShouldBe(TimerSkin.Digital);
        session.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void PomodoroSession_Complete_ShouldUpdateStatusAndDuration()
    {
        var session = PomodoroSession.Create(
            userId: Guid.NewGuid(),
            title: "Task Focus",
            category: null,
            sessionType: PomodoroSessionType.Work,
            plannedDurationSeconds: 1500,
            actualDurationSeconds: 0,
            startedAtUtc: DateTime.UtcNow,
            endedAtUtc: null,
            status: PomodoroStatus.InProgress);

        session.Complete(1500, "Done all planned work");

        session.Status.ShouldBe(PomodoroStatus.Completed);
        session.ActualDurationSeconds.ShouldBe(1500);
        session.Notes.ShouldBe("Done all planned work");
        session.EndedAtUtc.ShouldNotBeNull();
    }

    [Fact]
    public void PomodoroSession_Abandon_ShouldUpdateStatus()
    {
        var session = PomodoroSession.Create(
            userId: Guid.NewGuid(),
            title: "Urgent Interruption",
            category: null,
            sessionType: PomodoroSessionType.Work,
            plannedDurationSeconds: 1500,
            actualDurationSeconds: 0,
            startedAtUtc: DateTime.UtcNow,
            endedAtUtc: null,
            status: PomodoroStatus.InProgress);

        session.Abandon(350, "Had an emergency meeting");

        session.Status.ShouldBe(PomodoroStatus.Abandoned);
        session.ActualDurationSeconds.ShouldBe(350);
        session.Notes.ShouldBe("Had an emergency meeting");
    }

    [Fact]
    public void PomodoroSetting_CreateDefault_ShouldHaveExpectedRulesDefaults()
    {
        var userId = Guid.NewGuid();
        var setting = PomodoroSetting.CreateDefault(userId);

        setting.UserId.ShouldBe(userId);
        setting.WorkDurationMinutes.ShouldBe(PomodoroSetting.Rules.DefaultWorkDurationMinutes);
        setting.ShortBreakMinutes.ShouldBe(PomodoroSetting.Rules.DefaultShortBreakMinutes);
        setting.LongBreakMinutes.ShouldBe(PomodoroSetting.Rules.DefaultLongBreakMinutes);
        setting.LongBreakInterval.ShouldBe(PomodoroSetting.Rules.DefaultLongBreakInterval);
        setting.SoundEnabled.ShouldBeTrue();
    }

    [Fact]
    public void PomodoroSetting_Update_ShouldClampValuesWithinRules()
    {
        var setting = PomodoroSetting.CreateDefault(Guid.NewGuid());

        setting.Update(
            workDurationMinutes: 999, // Should clamp to WorkDurationMinutesMax (120)
            shortBreakMinutes: -10,  // Should clamp to ShortBreakMinutesMin (1)
            longBreakMinutes: 30,
            longBreakInterval: 50,  // Should clamp to LongBreakIntervalMax (20)
            autoStartBreaks: true,
            autoStartPomodoros: true,
            soundEnabled: false,
            defaultSkin: TimerSkin.Analog);

        setting.WorkDurationMinutes.ShouldBe(PomodoroSetting.Rules.WorkDurationMinutesMax);
        setting.ShortBreakMinutes.ShouldBe(PomodoroSetting.Rules.ShortBreakMinutesMin);
        setting.LongBreakMinutes.ShouldBe(30);
        setting.LongBreakInterval.ShouldBe(PomodoroSetting.Rules.LongBreakIntervalMax);
        setting.AutoStartBreaks.ShouldBeTrue();
        setting.DefaultSkin.ShouldBe(TimerSkin.Analog);
    }

    [Fact]
    public void CreatePomodoroSessionCommandValidator_ShouldRejectInvalidDurations()
    {
        var validator = new CreatePomodoroSessionCommandValidator();
        var invalidDto = new CreatePomodoroSessionDto(
            Title: "", // Empty title invalid
            Category: null,
            SessionType: PomodoroSessionType.Work,
            PlannedDurationSeconds: 0, // Less than min invalid
            ActualDurationSeconds: 0,
            StartedAtUtc: DateTime.UtcNow,
            EndedAtUtc: null,
            Status: PomodoroStatus.InProgress);

        var command = new CreatePomodoroSessionCommand(Guid.NewGuid(), invalidDto);
        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Dto.Title");
        result.Errors.ShouldContain(e => e.PropertyName == "Dto.PlannedDurationSeconds");
    }
}
