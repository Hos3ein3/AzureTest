using System;
using AzureTest.Domain.Entities.Trading.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Trading;

public sealed class TradingWatchlist : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int SymbolMinLength = 1;
        public const int SymbolMaxLength = 30;
        public const int NameMaxLength = 100;
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Symbol { get; private set; } = string.Empty;
    public string AssetName { get; private set; } = string.Empty;
    public AssetClass AssetClass { get; private set; }
    public DateTime AddedAtUtc { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private TradingWatchlist() { }

    private TradingWatchlist(Guid userId, Guid tenantId, string symbol, string assetName, AssetClass assetClass)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        Symbol = symbol.ToUpperInvariant();
        AssetName = assetName;
        AssetClass = assetClass;
        AddedAtUtc = DateTime.UtcNow;
    }

    public static TradingWatchlist Create(Guid userId, Guid tenantId, string symbol, string assetName, AssetClass assetClass)
    {
        return new TradingWatchlist(
            userId,
            tenantId,
            symbol.Trim(),
            string.IsNullOrWhiteSpace(assetName) ? symbol.Trim() : assetName.Trim(),
            assetClass);
    }
}
