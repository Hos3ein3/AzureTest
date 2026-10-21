namespace AzureTest.Domain.Entities.Forex.Enums;

public enum ForexExitReason
{
    None = 0,
    TakeProfit = 1,
    StopLoss = 2,
    TrailingStop = 3,
    BreakEven = 4,
    Manual = 5,
    MarginCall = 6,
    EmergencyStop = 7,
    SignalExit = 8
}
