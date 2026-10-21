using System;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Crypto;

public sealed class CryptoStrategy : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 1000;
        public const int SymbolMaxLength = 20;
        public const int ParametersMaxLength = 4000;
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public CryptoStrategyType StrategyType { get; private set; }
    public string Symbol { get; private set; } = "BTCUSDT";
    public CryptoTimeframe Timeframe { get; private set; } = CryptoTimeframe.H1;
    public string ParametersJson { get; private set; } = "{}";
    public bool IsActive { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private CryptoStrategy() { }

    private CryptoStrategy(
        Guid userId,
        Guid tenantId,
        string name,
        string description,
        CryptoStrategyType strategyType,
        string symbol,
        CryptoTimeframe timeframe,
        string parametersJson,
        bool isActive)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        Name = name;
        Description = description;
        StrategyType = strategyType;
        Symbol = symbol;
        Timeframe = timeframe;
        ParametersJson = parametersJson;
        IsActive = isActive;
    }

    public static CryptoStrategy Create(
        Guid userId,
        Guid tenantId,
        string name,
        string description,
        CryptoStrategyType strategyType,
        string symbol = "BTCUSDT",
        CryptoTimeframe timeframe = CryptoTimeframe.H1,
        string? parametersJson = null,
        bool isActive = true)
    {
        return new CryptoStrategy(
            userId,
            tenantId,
            name.Trim(),
            description.Trim(),
            strategyType,
            symbol.Trim().ToUpperInvariant(),
            timeframe,
            string.IsNullOrWhiteSpace(parametersJson) ? "{}" : parametersJson.Trim(),
            isActive);
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }

    public void UpdateParameters(string name, string description, string parametersJson, CryptoTimeframe timeframe)
    {
        Name = name.Trim();
        Description = description.Trim();
        ParametersJson = string.IsNullOrWhiteSpace(parametersJson) ? "{}" : parametersJson.Trim();
        Timeframe = timeframe;
    }
}
