using System;
using AzureTest.Domain.Enums.Pomodoro;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Pomodoro;

public sealed class PomodoroSetting : BaseEntity, IAudit
{
    public static class Rules
    {
        public const int WorkDurationMinutesMin = 1;
        public const int WorkDurationMinutesMax = 120;
        public const int DefaultWorkDurationMinutes = 25;

        public const int ShortBreakMinutesMin = 1;
        public const int ShortBreakMinutesMax = 60;
        public const int DefaultShortBreakMinutes = 5;

        public const int LongBreakMinutesMin = 1;
        public const int LongBreakMinutesMax = 120;
        public const int DefaultLongBreakMinutes = 15;

        public const int LongBreakIntervalMin = 1;
        public const int LongBreakIntervalMax = 20;
        public const int DefaultLongBreakInterval = 4;
    }

    private PomodoroSetting() { }

    public Guid UserId { get; private set; }
    public int WorkDurationMinutes { get; private set; } = Rules.DefaultWorkDurationMinutes;
    public int ShortBreakMinutes { get; private set; } = Rules.DefaultShortBreakMinutes;
    public int LongBreakMinutes { get; private set; } = Rules.DefaultLongBreakMinutes;
    public int LongBreakInterval { get; private set; } = Rules.DefaultLongBreakInterval;
    public bool AutoStartBreaks { get; private set; }
    public bool AutoStartPomodoros { get; private set; }
    public bool SoundEnabled { get; private set; } = true;
    public TimerSkin DefaultSkin { get; private set; } = TimerSkin.Digital;

    // IAudit
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    public static PomodoroSetting CreateDefault(Guid userId)
    {
        return new PomodoroSetting
        {
            UserId = userId,
            WorkDurationMinutes = Rules.DefaultWorkDurationMinutes,
            ShortBreakMinutes = Rules.DefaultShortBreakMinutes,
            LongBreakMinutes = Rules.DefaultLongBreakMinutes,
            LongBreakInterval = Rules.DefaultLongBreakInterval,
            AutoStartBreaks = false,
            AutoStartPomodoros = false,
            SoundEnabled = true,
            DefaultSkin = TimerSkin.Digital,
            CreatedOn = DateTime.UtcNow
        };
    }

    public void Update(
        int workDurationMinutes,
        int shortBreakMinutes,
        int longBreakMinutes,
        int longBreakInterval,
        bool autoStartBreaks,
        bool autoStartPomodoros,
        bool soundEnabled,
        TimerSkin defaultSkin)
    {
        WorkDurationMinutes = Math.Clamp(workDurationMinutes, Rules.WorkDurationMinutesMin, Rules.WorkDurationMinutesMax);
        ShortBreakMinutes = Math.Clamp(shortBreakMinutes, Rules.ShortBreakMinutesMin, Rules.ShortBreakMinutesMax);
        LongBreakMinutes = Math.Clamp(longBreakMinutes, Rules.LongBreakMinutesMin, Rules.LongBreakMinutesMax);
        LongBreakInterval = Math.Clamp(longBreakInterval, Rules.LongBreakIntervalMin, Rules.LongBreakIntervalMax);
        AutoStartBreaks = autoStartBreaks;
        AutoStartPomodoros = autoStartPomodoros;
        SoundEnabled = soundEnabled;
        DefaultSkin = defaultSkin;
        ModifiedOn = DateTime.UtcNow;
    }
}
