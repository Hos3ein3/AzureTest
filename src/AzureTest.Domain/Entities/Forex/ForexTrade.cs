using System;
using AzureTest.Domain.Entities.Forex.Enums;
using AzureTest.Domain.Entities.Forex.Events;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Forex;

public sealed class ForexTrade : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int SymbolMinLength = 3;
        public const int SymbolMaxLength = 20;
        public const int PricePrecision = 18;
        public const int PriceScale = 5;
        public const int LotSizePrecision = 18;
        public const int LotSizeScale = 2;
        public const int ProfitPrecision = 18;
        public const int ProfitScale = 2;
        public const int NotesMaxLength = 2000;
        public const int UrlMaxLength = 500;
        public static readonly decimal LotSizeMin = 0.01m;
        public static readonly decimal LotSizeMax = 100.00m;
        public static readonly decimal PriceMin = 0.00001m;
    }

    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public Guid? OrderId { get; private set; }
    public string Symbol { get; private set; } = string.Empty;
    public ForexTradeDirection Direction { get; private set; }
    public decimal LotSize { get; private set; }
    public decimal OpenPrice { get; private set; }
    public decimal? ClosePrice { get; private set; }
    public decimal? StopLoss { get; private set; }
    public decimal? TakeProfit { get; private set; }
    public decimal TrailingStopPips { get; private set; }
    public bool BreakEvenTriggered { get; private set; }
    public decimal MarginRequired { get; private set; }
    public decimal SwapCost { get; private set; }
    public decimal Commission { get; private set; }
    public decimal GrossProfit { get; private set; }
    public decimal NetProfit { get; private set; }
    public decimal Pips { get; private set; }
    public ForexTradeStatus Status { get; private set; }
    public ForexExitReason ExitReason { get; private set; }
    public DateTime OpenTimeUtc { get; private set; }
    public DateTime? CloseTimeUtc { get; private set; }
    public string? Notes { get; private set; }
    public string? ScreenshotUrl { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private ForexTrade() { }

    private ForexTrade(
        Guid accountId,
        Guid userId,
        Guid? orderId,
        string symbol,
        ForexTradeDirection direction,
        decimal lotSize,
        decimal openPrice,
        decimal? stopLoss,
        decimal? takeProfit,
        decimal trailingStopPips,
        decimal marginRequired,
        Guid? tenantId = null)
    {
        AccountId = accountId;
        UserId = userId;
        TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId;
        OrderId = orderId;
        Symbol = symbol.ToUpperInvariant().Trim();
        Direction = direction;
        LotSize = lotSize;
        OpenPrice = openPrice;
        StopLoss = stopLoss;
        TakeProfit = takeProfit;
        TrailingStopPips = trailingStopPips;
        MarginRequired = marginRequired;
        Status = ForexTradeStatus.Open;
        ExitReason = ForexExitReason.None;
        OpenTimeUtc = DateTime.UtcNow;
    }

    public static ForexTrade Create(
        Guid accountId,
        Guid userId,
        Guid? orderId,
        string symbol,
        ForexTradeDirection direction,
        decimal lotSize,
        decimal openPrice,
        decimal? stopLoss,
        decimal? takeProfit,
        decimal trailingStopPips = 0m,
        decimal marginRequired = 0m,
        Guid? tenantId = null)
    {
        return new ForexTrade(accountId, userId, orderId, symbol, direction, lotSize, openPrice, stopLoss, takeProfit, trailingStopPips, marginRequired, tenantId);
    }

    public void UpdateStops(decimal? stopLoss, decimal? takeProfit, decimal trailingStopPips)
    {
        StopLoss = stopLoss;
        TakeProfit = takeProfit;
        TrailingStopPips = trailingStopPips;
    }

    public void TriggerBreakEven(decimal breakEvenOffsetPips = 1m)
    {
        if (BreakEvenTriggered) return;

        // Move SL to open price plus minimal buffer
        var pipMultiplier = Symbol.EndsWith("JPY", StringComparison.OrdinalIgnoreCase) ? 0.01m : 0.0001m;
        var offset = breakEvenOffsetPips * pipMultiplier;

        StopLoss = Direction == ForexTradeDirection.Buy ? OpenPrice + offset : OpenPrice - offset;
        BreakEvenTriggered = true;
    }

    public void AdjustTrailingStop(decimal currentMarketPrice)
    {
        if (TrailingStopPips <= 0m) return;

        var pipMultiplier = Symbol.EndsWith("JPY", StringComparison.OrdinalIgnoreCase) ? 0.01m : 0.0001m;
        var distance = TrailingStopPips * pipMultiplier;

        if (Direction == ForexTradeDirection.Buy)
        {
            var newStop = currentMarketPrice - distance;
            if (StopLoss == null || newStop > StopLoss.Value)
            {
                StopLoss = Math.Round(newStop, Rules.PriceScale);
            }
        }
        else
        {
            var newStop = currentMarketPrice + distance;
            if (StopLoss == null || newStop < StopLoss.Value)
            {
                StopLoss = Math.Round(newStop, Rules.PriceScale);
            }
        }
    }

    public void AddJournalNote(string notes, string? screenshotUrl = null)
    {
        Notes = notes;
        if (!string.IsNullOrWhiteSpace(screenshotUrl))
        {
            ScreenshotUrl = screenshotUrl;
        }
    }

    public void Close(decimal closePrice, decimal pips, decimal grossProfit, decimal swap, decimal commission, ForexExitReason reason)
    {
        ClosePrice = closePrice;
        Pips = pips;
        GrossProfit = grossProfit;
        SwapCost = swap;
        Commission = commission;
        NetProfit = grossProfit - swap - commission;
        Status = ForexTradeStatus.Closed;
        ExitReason = reason;
        CloseTimeUtc = DateTime.UtcNow;

        AddDomainEvent(new ForexTradeClosedEvent(Id, AccountId, Symbol, NetProfit, reason));
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
