using System;
using AzureTest.Domain.Enums.Pomodoro;

namespace AzureTest.Contracts.DTOs.Pomodoro;

public sealed record PomodoroSettingDto(
    Guid Id,
    Guid UserId,
    int WorkDurationMinutes,
    int ShortBreakMinutes,
    int LongBreakMinutes,
    int LongBreakInterval,
    bool AutoStartBreaks,
    bool AutoStartPomodoros,
    bool SoundEnabled,
    TimerSkin DefaultSkin);

public sealed record UpdatePomodoroSettingDto(
    int WorkDurationMinutes,
    int ShortBreakMinutes,
    int LongBreakMinutes,
    int LongBreakInterval,
    bool AutoStartBreaks,
    bool AutoStartPomodoros,
    bool SoundEnabled,
    TimerSkin DefaultSkin);
