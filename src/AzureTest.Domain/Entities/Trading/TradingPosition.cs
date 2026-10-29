using System;
using AzureTest.Domain.Entities.Trading.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Trading;

public sealed class TradingPosition : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int SymbolMinLength = 1;
        public const int SymbolMaxLength = 30;
        public const int NameMaxLength = 100;
        public const int JsonMaxLength = 2000;
        public const int PricePrecision = 18;
        public const int PriceScale = 6;
        public const int QuantityPrecision = 18;
        public const int QuantityScale = 6;
        public const int PnlPrecision = 18;
        public const int PnlScale = 4;
        public const int LeverageMin = 1;
        public const int LeverageMax = 250;
        public const int DefaultLeverage = 1;
        public static readonly decimal ZeroValue = 0.00m;
        public static readonly decimal MinQuantity = 0.000001m;
        public static readonly decimal MinPrice = 0.000001m;
    }

    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Symbol { get; private set; } = string.Empty;
    public string AssetName { get; private set; } = string.Empty;
    public AssetClass AssetClass { get; private set; }
    public TradingOrderSide Side { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal EntryPrice { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public decimal? ClosePrice { get; private set; }
    public int Leverage { get; private set; } = Rules.DefaultLeverage;
    public decimal InitialMargin { get; private set; }
    public decimal UnrealizedPnl { get; private set; }
    public decimal RealizedPnl { get; private set; }
    public decimal? StopLoss { get; private set; }
    public decimal? TakeProfit { get; private set; }
    public string? MultipleTakeProfitsJson { get; private set; }
    public decimal? TrailingStopDistance { get; private set; }
    public decimal? HighestPriceReached { get; private set; }
    public decimal? LowestPriceReached { get; private set; }
    public TradingPositionStatus Status { get; private set; } = TradingPositionStatus.Open;
    public DateTime OpenedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private TradingPosition() { }

    private TradingPosition(
        Guid accountId,
        Guid userId,
        Guid tenantId,
        string symbol,
        string assetName,
        AssetClass assetClass,
        TradingOrderSide side,
        decimal quantity,
        decimal entryPrice,
        int leverage,
        decimal initialMargin,
        decimal? stopLoss,
        decimal? takeProfit,
        string? multipleTakeProfitsJson,
        decimal? trailingStopDistance)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        UserId = userId;
        TenantId = tenantId;
        Symbol = symbol.ToUpperInvariant();
        AssetName = assetName;
        AssetClass = assetClass;
        Side = side;
        Quantity = quantity;
        EntryPrice = entryPrice;
        CurrentPrice = entryPrice;
        Leverage = leverage;
        InitialMargin = initialMargin;
        StopLoss = stopLoss;
        TakeProfit = takeProfit;
        MultipleTakeProfitsJson = multipleTakeProfitsJson;
        TrailingStopDistance = trailingStopDistance;
        HighestPriceReached = entryPrice;
        LowestPriceReached = entryPrice;
        UnrealizedPnl = Rules.ZeroValue;
        RealizedPnl = Rules.ZeroValue;
        Status = TradingPositionStatus.Open;
        OpenedAtUtc = DateTime.UtcNow;
    }

    public static TradingPosition Open(
        Guid accountId,
        Guid userId,
        Guid tenantId,
        string symbol,
        string assetName,
        AssetClass assetClass,
        TradingOrderSide side,
        decimal quantity,
        decimal entryPrice,
        int leverage = Rules.DefaultLeverage,
        decimal? stopLoss = null,
        decimal? takeProfit = null,
        string? multipleTakeProfitsJson = null,
        decimal? trailingStopDistance = null)
    {
        var lev = Math.Clamp(leverage, Rules.LeverageMin, Rules.LeverageMax);
        var notional = quantity * entryPrice;
        var margin = notional / lev;

        return new TradingPosition(
            accountId,
            userId,
            tenantId,
            symbol.Trim(),
            string.IsNullOrWhiteSpace(assetName) ? symbol.Trim() : assetName.Trim(),
            assetClass,
            side,
            quantity,
            entryPrice,
            lev,
            margin,
            stopLoss,
            takeProfit,
            multipleTakeProfitsJson,
            trailingStopDistance);
    }

    public void UpdateCurrentPrice(decimal latestPrice)
    {
        if (Status != TradingPositionStatus.Open || latestPrice <= 0) return;

        CurrentPrice = latestPrice;

        if (HighestPriceReached == null || latestPrice > HighestPriceReached)
            HighestPriceReached = latestPrice;

        if (LowestPriceReached == null || latestPrice < LowestPriceReached)
            LowestPriceReached = latestPrice;

        // Calculate Unrealized PnL
        if (Side == TradingOrderSide.Buy)
        {
            UnrealizedPnl = (latestPrice - EntryPrice) * Quantity;
        }
        else
        {
            UnrealizedPnl = (EntryPrice - latestPrice) * Quantity;
        }

        // Handle trailing stop if set
        if (TrailingStopDistance.HasValue && TrailingStopDistance.Value > 0)
        {
            if (Side == TradingOrderSide.Buy)
            {
                var trailSL = HighestPriceReached!.Value - TrailingStopDistance.Value;
                if (!StopLoss.HasValue || trailSL > StopLoss.Value)
                    StopLoss = trailSL;
            }
            else
            {
                var trailSL = LowestPriceReached!.Value + TrailingStopDistance.Value;
                if (!StopLoss.HasValue || trailSL < StopLoss.Value)
                    StopLoss = trailSL;
            }
        }
    }

    public void UpdateStops(decimal? stopLoss, decimal? takeProfit, decimal? trailingStopDistance, string? multipleTakeProfitsJson = null)
    {
        StopLoss = stopLoss;
        TakeProfit = takeProfit;
        TrailingStopDistance = trailingStopDistance;
        MultipleTakeProfitsJson = (takeProfit.HasValue && !string.IsNullOrWhiteSpace(multipleTakeProfitsJson))
            ? multipleTakeProfitsJson
            : null;
    }

    public void Close(decimal closePrice, string triggerReason = "Manual")
    {
        if (Status != TradingPositionStatus.Open) return;

        ClosePrice = closePrice;
        CurrentPrice = closePrice;
        Status = TradingPositionStatus.Closed;
        ClosedAtUtc = DateTime.UtcNow;

        if (Side == TradingOrderSide.Buy)
        {
            RealizedPnl = (closePrice - EntryPrice) * Quantity;
        }
        else
        {
            RealizedPnl = (EntryPrice - closePrice) * Quantity;
        }

        // In isolated paper trading, loss cannot exceed the allocated initial margin
        if (RealizedPnl < -InitialMargin)
        {
            RealizedPnl = -InitialMargin;
        }

        UnrealizedPnl = Rules.ZeroValue;

        var roi = InitialMargin > 0 ? (RealizedPnl / InitialMargin) * 100m : 0m;
        AddDomainEvent(new Events.TradingPositionClosedDomainEvent(
            Id,
            AccountId,
            UserId,
            TenantId,
            Symbol,
            AssetName,
            AssetClass,
            Side,
            Quantity,
            EntryPrice,
            closePrice,
            InitialMargin,
            RealizedPnl,
            roi,
            triggerReason,
            ClosedAtUtc.Value));
    }

    public void Liquidate(decimal liquidationPrice)
    {
        if (Status != TradingPositionStatus.Open) return;

        ClosePrice = liquidationPrice;
        CurrentPrice = liquidationPrice;
        Status = TradingPositionStatus.Liquidated;
        ClosedAtUtc = DateTime.UtcNow;
        RealizedPnl = -InitialMargin;
        UnrealizedPnl = Rules.ZeroValue;

        AddDomainEvent(new Events.TradingPositionLiquidatedDomainEvent(
            Id,
            AccountId,
            UserId,
            TenantId,
            Symbol,
            AssetName,
            AssetClass,
            Side,
            Quantity,
            EntryPrice,
            liquidationPrice,
            InitialMargin,
            ClosedAtUtc.Value));
    }
}
