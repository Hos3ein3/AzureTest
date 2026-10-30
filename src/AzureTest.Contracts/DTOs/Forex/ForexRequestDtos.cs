using System;
using AzureTest.Domain.Entities.Forex.Enums;

namespace AzureTest.Contracts.DTOs.Forex;

public sealed class ForexChartRequestDto
{
    public string Symbol { get; set; } = "EUR/USD";
    public ForexTimeframe Timeframe { get; set; } = ForexTimeframe.H1;

    public ForexChartRequestDto() { }
    public ForexChartRequestDto(string symbol, ForexTimeframe timeframe)
    {
        Symbol = symbol;
        Timeframe = timeframe;
    }
}

public sealed class ForexAnalysisRequestDto
{
    public string Symbol { get; set; } = "EUR/USD";
    public ForexTimeframe Timeframe { get; set; } = ForexTimeframe.H1;

    public ForexAnalysisRequestDto() { }
    public ForexAnalysisRequestDto(string symbol, ForexTimeframe timeframe)
    {
        Symbol = symbol;
        Timeframe = timeframe;
    }
}

public sealed class PartialCloseForexTradeRequestDto
{
    public Guid TradeId { get; set; }
    public decimal LotsToClose { get; set; }

    public PartialCloseForexTradeRequestDto() { }
    public PartialCloseForexTradeRequestDto(Guid tradeId, decimal lotsToClose)
    {
        TradeId = tradeId;
        LotsToClose = lotsToClose;
    }
}

public sealed class ForexKillSwitchRequestDto
{
    public string? Reason { get; set; }

    public ForexKillSwitchRequestDto() { }
    public ForexKillSwitchRequestDto(string? reason)
    {
        Reason = reason;
    }
}
