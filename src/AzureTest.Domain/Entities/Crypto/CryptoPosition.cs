using System;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.Domain.Entities.Crypto.Events;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Crypto;

public sealed class CryptoPosition : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int SymbolMinLength = 3;
        public const int SymbolMaxLength = 20;
        public const int PricePrecision = 18;
        public const int PriceScale = 8;
        public const int QuantityPrecision = 18;
        public const int QuantityScale = 8;
        public const int PnlPrecision = 18;
        public const int PnlScale = 4;
        public const int LeverageMin = 1;
        public const int LeverageMax = 50;
        public const int DefaultLeverage = 1;
        public static readonly decimal ZeroValue = 0.00m;
        public static readonly decimal MaintenanceMarginRatio = 0.005m; // 0.5% standard maintenance margin
    }

    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Symbol { get; private set; } = string.Empty;
    public CryptoPositionSide Side { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal EntryPrice { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public decimal? ClosePrice { get; private set; }
    public int Leverage { get; private set; } = Rules.DefaultLeverage;
    public decimal InitialMargin { get; private set; }
    public decimal LiquidationPrice { get; private set; }
    public decimal UnrealizedPnl { get; private set; }
    public decimal RealizedPnl { get; private set; }
    public decimal? StopLoss { get; private set; }
    public decimal? TakeProfit { get; private set; }
    public decimal? TrailingStopDistance { get; private set; }
    public decimal? HighestPriceReached { get; private set; }
    public decimal? LowestPriceReached { get; private set; }
    public CryptoPositionStatus Status { get; private set; } = CryptoPositionStatus.Open;
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

    private CryptoPosition() { }

    private CryptoPosition(
        Guid accountId,
        Guid userId,
        Guid tenantId,
        string symbol,
        CryptoPositionSide side,
        decimal quantity,
        decimal entryPrice,
        int leverage,
        decimal? stopLoss,
        decimal? takeProfit,
        decimal? trailingStopDistance)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        UserId = userId;
        TenantId = tenantId;
        Symbol = symbol;
        Side = side;
        Quantity = quantity;
        EntryPrice = entryPrice;
        CurrentPrice = entryPrice;
        Leverage = Math.Clamp(leverage, Rules.LeverageMin, Rules.LeverageMax);
        InitialMargin = (quantity * entryPrice) / Leverage;
        StopLoss = stopLoss;
        TakeProfit = takeProfit;
        TrailingStopDistance = trailingStopDistance;
        HighestPriceReached = entryPrice;
        LowestPriceReached = entryPrice;
        Status = CryptoPositionStatus.Open;
        OpenedAtUtc = DateTime.UtcNow;
        UnrealizedPnl = Rules.ZeroValue;
        RealizedPnl = Rules.ZeroValue;

        CalculateLiquidationPrice();
    }

    public static CryptoPosition Create(
        Guid accountId,
        Guid userId,
        Guid tenantId,
        string symbol,
        CryptoPositionSide side,
        decimal quantity,
        decimal entryPrice,
        int leverage = Rules.DefaultLeverage,
        decimal? stopLoss = null,
        decimal? takeProfit = null,
        decimal? trailingStopDistance = null)
    {
        return new CryptoPosition(
            accountId,
            userId,
            tenantId,
            symbol.Trim().ToUpperInvariant(),
            side,
            quantity,
            entryPrice,
            leverage,
            stopLoss,
            takeProfit,
            trailingStopDistance);
    }

    private void CalculateLiquidationPrice()
    {
        if (Leverage <= 1)
        {
            LiquidationPrice = Side == CryptoPositionSide.Long ? Rules.ZeroValue : EntryPrice * 2.0m;
            return;
        }

        // Standard perpetual futures liquidation calculation
        // Long: EntryPrice * (1 - (1/Leverage) + MaintenanceMargin)
        // Short: EntryPrice * (1 + (1/Leverage) - MaintenanceMargin)
        decimal leverageRatio = 1.0m / Leverage;
        if (Side == CryptoPositionSide.Long)
        {
            LiquidationPrice = Math.Max(Rules.ZeroValue, EntryPrice * (1.0m - leverageRatio + Rules.MaintenanceMarginRatio));
        }
        else
        {
            LiquidationPrice = EntryPrice * (1.0m + leverageRatio - Rules.MaintenanceMarginRatio);
        }
    }

    public void UpdatePrice(decimal marketPrice)
    {
        if (Status != CryptoPositionStatus.Open) return;

        CurrentPrice = marketPrice;

        if (Side == CryptoPositionSide.Long)
        {
            UnrealizedPnl = (CurrentPrice - EntryPrice) * Quantity;
            if (CurrentPrice > (HighestPriceReached ?? EntryPrice))
                HighestPriceReached = CurrentPrice;

            // Trailing Stop check
            if (TrailingStopDistance.HasValue && HighestPriceReached.HasValue)
            {
                var dynamicStop = HighestPriceReached.Value - TrailingStopDistance.Value;
                if (!StopLoss.HasValue || dynamicStop > StopLoss.Value)
                    StopLoss = dynamicStop;
            }

            // Liquidation check
            if (CurrentPrice <= LiquidationPrice && Leverage > 1)
            {
                Liquidate(marketPrice);
                return;
            }
        }
        else
        {
            UnrealizedPnl = (EntryPrice - CurrentPrice) * Quantity;
            if (CurrentPrice < (LowestPriceReached ?? EntryPrice))
                LowestPriceReached = CurrentPrice;

            // Trailing Stop check
            if (TrailingStopDistance.HasValue && LowestPriceReached.HasValue)
            {
                var dynamicStop = LowestPriceReached.Value + TrailingStopDistance.Value;
                if (!StopLoss.HasValue || dynamicStop < StopLoss.Value)
                    StopLoss = dynamicStop;
            }

            // Liquidation check
            if (CurrentPrice >= LiquidationPrice && Leverage > 1)
            {
                Liquidate(marketPrice);
                return;
            }
        }
    }

    public void Close(decimal closePrice)
    {
        ClosePrice = closePrice;
        Status = CryptoPositionStatus.Closed;
        ClosedAtUtc = DateTime.UtcNow;

        if (Side == CryptoPositionSide.Long)
        {
            RealizedPnl = (closePrice - EntryPrice) * Quantity;
        }
        else
        {
            RealizedPnl = (EntryPrice - closePrice) * Quantity;
        }

        UnrealizedPnl = Rules.ZeroValue;
        var returnPct = InitialMargin > 0 ? (RealizedPnl / InitialMargin) * 100m : Rules.ZeroValue;
        AddDomainEvent(new CryptoTradeClosedEvent(Id, AccountId, Symbol, RealizedPnl, returnPct));
    }

    public void Liquidate(decimal liquidationMarketPrice)
    {
        ClosePrice = liquidationMarketPrice;
        Status = CryptoPositionStatus.Liquidated;
        ClosedAtUtc = DateTime.UtcNow;
        RealizedPnl = -InitialMargin; // Total loss of margin
        UnrealizedPnl = Rules.ZeroValue;
        AddDomainEvent(new CryptoTradeClosedEvent(Id, AccountId, Symbol, RealizedPnl, -100.00m));
    }

    public void UpdateStops(decimal? stopLoss, decimal? takeProfit, decimal? trailingStopDistance)
    {
        StopLoss = stopLoss;
        TakeProfit = takeProfit;
        TrailingStopDistance = trailingStopDistance;
    }

    public void TopUpMargin(decimal addedMarginUsd)
    {
        if (Status != CryptoPositionStatus.Open || addedMarginUsd <= 0 || Quantity <= 0) return;

        InitialMargin += addedMarginUsd;
        decimal marginShiftPerUnit = addedMarginUsd / Quantity;
        if (Side == CryptoPositionSide.Long)
        {
            LiquidationPrice = Math.Max(Rules.ZeroValue, LiquidationPrice - marginShiftPerUnit);
        }
        else
        {
            LiquidationPrice = LiquidationPrice + marginShiftPerUnit;
        }
    }

    public decimal TrimPosition(decimal trimQuantity)
    {
        if (Status != CryptoPositionStatus.Open || trimQuantity <= 0) return 0m;

        var actualTrim = Math.Min(Quantity, trimQuantity);
        decimal portionPnl = (Side == CryptoPositionSide.Long ? (CurrentPrice - EntryPrice) : (EntryPrice - CurrentPrice)) * actualTrim;
        RealizedPnl += portionPnl;

        Quantity -= actualTrim;
        if (Quantity <= 0.00000001m)
        {
            Quantity = 0m;
            Close(CurrentPrice);
        }
        else
        {
            InitialMargin = (Quantity * EntryPrice) / Math.Max(1, Leverage);
            CalculateLiquidationPrice();
            if (Side == CryptoPositionSide.Long)
            {
                UnrealizedPnl = (CurrentPrice - EntryPrice) * Quantity;
            }
            else
            {
                UnrealizedPnl = (EntryPrice - CurrentPrice) * Quantity;
            }
        }

        return actualTrim;
    }
}
