using System;
using AzureTest.Domain.Entities.Forex.Enums;
using AzureTest.Domain.Entities.Forex.Events;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Forex;

public sealed class ForexOrder : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int SymbolMinLength = 3;
        public const int SymbolMaxLength = 20;
        public const int IdempotencyKeyMaxLength = 100;
        public const int NoteMaxLength = 500;
        public const int PricePrecision = 18;
        public const int PriceScale = 5;
        public const int LotSizePrecision = 18;
        public const int LotSizeScale = 2;
        public static readonly decimal LotSizeMin = 0.01m;
        public static readonly decimal LotSizeMax = 100.00m;
        public static readonly decimal PriceMin = 0.00001m;
    }

    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Symbol { get; private set; } = string.Empty;
    public ForexTradeDirection Direction { get; private set; }
    public ForexOrderType OrderType { get; private set; }
    public decimal LotSize { get; private set; }
    public decimal RequestedPrice { get; private set; }
    public decimal? FillPrice { get; private set; }
    public decimal? StopLoss { get; private set; }
    public decimal? TakeProfit { get; private set; }
    public decimal SlippagePips { get; private set; }
    public ForexOrderStatus Status { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
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

    private ForexOrder() { }

    private ForexOrder(
        Guid accountId,
        Guid userId,
        string symbol,
        ForexTradeDirection direction,
        ForexOrderType orderType,
        decimal lotSize,
        decimal requestedPrice,
        decimal? stopLoss,
        decimal? takeProfit,
        string idempotencyKey,
        Guid? tenantId = null)
    {
        AccountId = accountId;
        UserId = userId;
        TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId;
        Symbol = symbol.ToUpperInvariant().Trim();
        Direction = direction;
        OrderType = orderType;
        LotSize = lotSize;
        RequestedPrice = requestedPrice;
        StopLoss = stopLoss;
        TakeProfit = takeProfit;
        Status = ForexOrderStatus.Pending;
        IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? Guid.NewGuid().ToString("N") : idempotencyKey.Trim();

        AddDomainEvent(new ForexOrderPlacedEvent(Id, accountId, Symbol, lotSize));
    }

    public static ForexOrder Create(
        Guid accountId,
        Guid userId,
        string symbol,
        ForexTradeDirection direction,
        ForexOrderType orderType,
        decimal lotSize,
        decimal requestedPrice,
        decimal? stopLoss,
        decimal? takeProfit,
        string idempotencyKey,
        Guid? tenantId = null)
    {
        return new ForexOrder(accountId, userId, symbol, direction, orderType, lotSize, requestedPrice, stopLoss, takeProfit, idempotencyKey, tenantId);
    }

    public void MarkFilled(decimal fillPrice, decimal slippagePips)
    {
        Status = ForexOrderStatus.Filled;
        FillPrice = fillPrice;
        SlippagePips = slippagePips;
        FilledAtUtc = DateTime.UtcNow;
    }

    public void MarkCancelled(string reason = "Cancelled by user")
    {
        Status = ForexOrderStatus.Cancelled;
        RejectionReason = reason;
        CancelledAtUtc = DateTime.UtcNow;
    }

    public void MarkRejected(string reason)
    {
        Status = ForexOrderStatus.Rejected;
        RejectionReason = reason;
        CancelledAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
