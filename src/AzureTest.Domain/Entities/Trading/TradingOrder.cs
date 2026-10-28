using System;
using AzureTest.Domain.Entities.Trading.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Trading;

public sealed class TradingOrder : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
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
        public const int LeverageMin = 1;
        public const int LeverageMax = 250;
        public const int DefaultLeverage = 1;
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
    public TradingOrderType OrderType { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal? Price { get; private set; }
    public decimal? TriggerPrice { get; private set; }
    public int Leverage { get; private set; } = Rules.DefaultLeverage;
    public decimal? StopLoss { get; private set; }
    public decimal? TakeProfit { get; private set; }
    public string? MultipleTakeProfitsJson { get; private set; }
    public decimal? TrailingStopDistance { get; private set; }
    public TimeInForce TimeInForce { get; private set; } = TimeInForce.GTC;
    public DateTime? ExpiryDateUtc { get; private set; }
    public TradingOrderStatus Status { get; private set; } = TradingOrderStatus.Pending;
    public DateTime PlacedAtUtc { get; private set; }
    public DateTime? FilledAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private TradingOrder() { }

    private TradingOrder(
        Guid accountId,
        Guid userId,
        Guid tenantId,
        string symbol,
        string assetName,
        AssetClass assetClass,
        TradingOrderSide side,
        TradingOrderType orderType,
        decimal quantity,
        decimal? price,
        decimal? triggerPrice,
        int leverage,
        decimal? stopLoss,
        decimal? takeProfit,
        string? multipleTakeProfitsJson,
        decimal? trailingStopDistance,
        TimeInForce timeInForce,
        DateTime? expiryDateUtc)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        UserId = userId;
        TenantId = tenantId;
        Symbol = symbol.ToUpperInvariant();
        AssetName = assetName;
        AssetClass = assetClass;
        Side = side;
        OrderType = orderType;
        Quantity = quantity;
        Price = price;
        TriggerPrice = triggerPrice;
        Leverage = leverage;
        StopLoss = stopLoss;
        TakeProfit = takeProfit;
        MultipleTakeProfitsJson = multipleTakeProfitsJson;
        TrailingStopDistance = trailingStopDistance;
        TimeInForce = timeInForce;
        ExpiryDateUtc = expiryDateUtc;
        Status = TradingOrderStatus.Pending;
        PlacedAtUtc = DateTime.UtcNow;
    }

    public static TradingOrder Create(
        Guid accountId,
        Guid userId,
        Guid tenantId,
        string symbol,
        string assetName,
        AssetClass assetClass,
        TradingOrderSide side,
        TradingOrderType orderType,
        decimal quantity,
        decimal? price = null,
        decimal? triggerPrice = null,
        int leverage = Rules.DefaultLeverage,
        decimal? stopLoss = null,
        decimal? takeProfit = null,
        string? multipleTakeProfitsJson = null,
        decimal? trailingStopDistance = null,
        TimeInForce timeInForce = TimeInForce.GTC,
        DateTime? expiryDateUtc = null)
    {
        var lev = Math.Clamp(leverage, Rules.LeverageMin, Rules.LeverageMax);

        return new TradingOrder(
            accountId,
            userId,
            tenantId,
            symbol.Trim(),
            string.IsNullOrWhiteSpace(assetName) ? symbol.Trim() : assetName.Trim(),
            assetClass,
            side,
            orderType,
            quantity,
            price,
            triggerPrice,
            lev,
            stopLoss,
            takeProfit,
            multipleTakeProfitsJson,
            trailingStopDistance,
            timeInForce,
            expiryDateUtc);
    }

    public void Fill()
    {
        Status = TradingOrderStatus.Filled;
        FilledAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = TradingOrderStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
    }

    public void Expire()
    {
        Status = TradingOrderStatus.Expired;
    }
}
