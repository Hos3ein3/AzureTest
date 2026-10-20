using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AzureTest.Domain.Enums.Pomodoro;

public enum PomodoroStatus
{
    [Display(Name = "In Progress")]
    [Description("Timer currently running or paused")]
    InProgress = 1,

    [Display(Name = "Completed")]
    [Description("Session finished all planned duration successfully")]
    Completed = 2,

    [Display(Name = "Abandoned")]
    [Description("Session ended early or was stopped")]
    Abandoned = 3
}
