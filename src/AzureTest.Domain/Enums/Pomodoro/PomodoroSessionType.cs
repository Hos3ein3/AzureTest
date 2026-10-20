using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AzureTest.Domain.Enums.Pomodoro;

public enum PomodoroSessionType
{
    [Display(Name = "Focus Session")]
    [Description("Deep focus work interval")]
    Work = 1,

    [Display(Name = "Short Break")]
    [Description("Quick rest interval to recharge")]
    ShortBreak = 2,

    [Display(Name = "Long Break")]
    [Description("Extended rest interval after completing cycles")]
    LongBreak = 3
}
