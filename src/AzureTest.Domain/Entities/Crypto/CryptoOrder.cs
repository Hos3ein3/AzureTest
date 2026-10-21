using System;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.Domain.Entities.Crypto.Events;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Crypto;

public sealed class CryptoOrder : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int SymbolMinLength = 3;
        public const int SymbolMaxLength = 20;
        public const int IdempotencyKeyMaxLength = 100;
        public const int ClientOrderIdMaxLength = 100;
        public const int RejectionReasonMaxLength = 500;
        public const int PricePrecision = 18;
        public const int PriceScale = 8;
        public const int QuantityPrecision = 18;
        public const int QuantityScale = 8;
        public const int CommissionPrecision = 18;
        public const int CommissionScale = 8;
        public static readonly decimal MinPrice = 0.00000001m;
        public static readonly decimal MinQuantity = 0.00000001m;
        public static readonly decimal ZeroValue = 0.00m;
    }

    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Symbol { get; private set; } = string.Empty;
    public CryptoOrderSide Side { get; private set; }
    public CryptoOrderType OrderType { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal FilledQuantity { get; private set; }
    public decimal RequestedPrice { get; private set; }
    public decimal? StopPrice { get; private set; }
    public decimal? FillPrice { get; private set; }
    public decimal Commission { get; private set; }
    public decimal Slippage { get; private set; }
    public CryptoOrderStatus Status { get; private set; }
    public CryptoTradingMode TradingMode { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string ClientOrderId { get; private set; } = string.Empty;
    public string? RejectionReason { get; private set; }
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

    private CryptoOrder() { }

    private CryptoOrder(
        Guid accountId,
        Guid userId,
        Guid tenantId,
        string symbol,
        CryptoOrderSide side,
        CryptoOrderType orderType,
        decimal quantity,
        decimal requestedPrice,
        decimal? stopPrice,
        string idempotencyKey,
        string clientOrderId,
        CryptoTradingMode tradingMode)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        UserId = userId;
        TenantId = tenantId;
        Symbol = symbol;
        Side = side;
        OrderType = orderType;
        Quantity = quantity;
        RequestedPrice = requestedPrice;
        StopPrice = stopPrice;
        Status = CryptoOrderStatus.Pending;
        TradingMode = tradingMode;
        IdempotencyKey = idempotencyKey;
        ClientOrderId = string.IsNullOrWhiteSpace(clientOrderId) ? Guid.NewGuid().ToString("N") : clientOrderId;
        FilledQuantity = Rules.ZeroValue;
        Commission = Rules.ZeroValue;
        Slippage = Rules.ZeroValue;
    }

    public static CryptoOrder Create(
        Guid accountId,
        Guid userId,
        Guid tenantId,
        string symbol,
        CryptoOrderSide side,
        CryptoOrderType orderType,
        decimal quantity,
        decimal requestedPrice,
        decimal? stopPrice = null,
        string? idempotencyKey = null,
        string? clientOrderId = null,
        CryptoTradingMode tradingMode = CryptoTradingMode.PaperTrading)
    {
        var safeKey = string.IsNullOrWhiteSpace(idempotencyKey) ? Guid.NewGuid().ToString("N") : idempotencyKey.Trim();
        var safeClientOrderId = string.IsNullOrWhiteSpace(clientOrderId) ? Guid.NewGuid().ToString("N") : clientOrderId.Trim();

        var order = new CryptoOrder(
            accountId,
            userId,
            tenantId,
            symbol.Trim().ToUpperInvariant(),
            side,
            orderType,
            quantity,
            requestedPrice,
            stopPrice,
            safeKey,
            safeClientOrderId,
            tradingMode);

        order.AddDomainEvent(new CryptoOrderPlacedEvent(order.Id, accountId, order.Symbol, quantity, requestedPrice));
        return order;
    }

    public void Fill(decimal fillPrice, decimal filledQuantity, decimal commission, decimal slippage)
    {
        FillPrice = fillPrice;
        FilledQuantity = filledQuantity;
        Commission = commission;
        Slippage = slippage;
        Status = CryptoOrderStatus.Filled;
        FilledAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = CryptoOrderStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        Status = CryptoOrderStatus.Rejected;
        RejectionReason = reason;
        CancelledAtUtc = DateTime.UtcNow;
    }
}
