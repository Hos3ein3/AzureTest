using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AzureTest.Domain.Enums.Pomodoro;

public enum TimerSkin
{
    [Display(Name = "Digital LED")]
    [Description("Clean digital cyber countdown timer with progress ring")]
    Digital = 1,

    [Display(Name = "Analog Clock")]
    [Description("Classic clock face with rotating minute hand and elapsed slice")]
    Analog = 2,

    [Display(Name = "Sand Glass")]
    [Description("Vintage animated hourglass with falling sand grains")]
    SandGlass = 3,

    [Display(Name = "Kitchen Timer")]
    [Description("Mechanical round egg timer with turnable dial")]
    KitchenTimer = 4,

    [Display(Name = "Candle")]
    [Description("Atmospheric burning candle with flickering flame and melting wax")]
    Candle = 5
}
