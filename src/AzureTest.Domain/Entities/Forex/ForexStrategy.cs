using System;
using AzureTest.Domain.Entities.Forex.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Forex;

public sealed class ForexStrategy : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int NameMinLength = 3;
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 1000;
        public const int SymbolMaxLength = 20;
        public const int JsonParametersMaxLength = 4000;
        public const int DecimalPrecision = 18;
        public const int DecimalScale = 2;
        public static readonly decimal RiskPercentMin = 0.1m;
        public static readonly decimal RiskPercentMax = 10.0m;
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ForexStrategyType StrategyType { get; private set; }
    public string DefaultSymbol { get; private set; } = "EUR/USD";
    public ForexTimeframe DefaultTimeframe { get; private set; } = ForexTimeframe.H1;
    public decimal RiskPercent { get; private set; } = 1.0m;
    public decimal StopLossPips { get; private set; } = 25m;
    public decimal TakeProfitPips { get; private set; } = 50m;
    public bool UseTrailingStop { get; private set; }
    public decimal TrailingStopPips { get; private set; }
    public bool BreakEvenAtPipsEnabled { get; private set; }
    public decimal BreakEvenTriggerPips { get; private set; }
    public string ParametersJson { get; private set; } = "{}";
    public bool IsActive { get; private set; } = true;

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private ForexStrategy() { }

    private ForexStrategy(
        Guid userId,
        string name,
        string description,
        ForexStrategyType strategyType,
        string defaultSymbol,
        ForexTimeframe defaultTimeframe,
        decimal riskPercent,
        decimal stopLossPips,
        decimal takeProfitPips,
        bool useTrailingStop,
        decimal trailingStopPips,
        bool breakEvenAtPipsEnabled,
        decimal breakEvenTriggerPips,
        string parametersJson,
        Guid? tenantId = null)
    {
        UserId = userId;
        TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId;
        Name = name.Trim();
        Description = description.Trim();
        StrategyType = strategyType;
        DefaultSymbol = string.IsNullOrWhiteSpace(defaultSymbol) ? "EUR/USD" : defaultSymbol.ToUpperInvariant().Trim();
        DefaultTimeframe = defaultTimeframe;
        RiskPercent = riskPercent;
        StopLossPips = stopLossPips;
        TakeProfitPips = takeProfitPips;
        UseTrailingStop = useTrailingStop;
        TrailingStopPips = trailingStopPips;
        BreakEvenAtPipsEnabled = breakEvenAtPipsEnabled;
        BreakEvenTriggerPips = breakEvenTriggerPips;
        ParametersJson = string.IsNullOrWhiteSpace(parametersJson) ? "{}" : parametersJson.Trim();
        IsActive = true;
    }

    public static ForexStrategy Create(
        Guid userId,
        string name,
        string description,
        ForexStrategyType strategyType,
        string defaultSymbol,
        ForexTimeframe defaultTimeframe,
        decimal riskPercent,
        decimal stopLossPips,
        decimal takeProfitPips,
        bool useTrailingStop = false,
        decimal trailingStopPips = 0m,
        bool breakEvenAtPipsEnabled = false,
        decimal breakEvenTriggerPips = 0m,
        string parametersJson = "{}",
        Guid? tenantId = null)
    {
        return new ForexStrategy(
            userId, name, description, strategyType, defaultSymbol, defaultTimeframe,
            riskPercent, stopLossPips, takeProfitPips, useTrailingStop, trailingStopPips,
            breakEvenAtPipsEnabled, breakEvenTriggerPips, parametersJson, tenantId);
    }

    public void UpdateConfiguration(
        string name,
        string description,
        string defaultSymbol,
        ForexTimeframe defaultTimeframe,
        decimal riskPercent,
        decimal stopLossPips,
        decimal takeProfitPips,
        bool useTrailingStop,
        decimal trailingStopPips,
        bool breakEvenAtPipsEnabled,
        decimal breakEvenTriggerPips,
        string parametersJson,
        bool isActive)
    {
        Name = name.Trim();
        Description = description.Trim();
        DefaultSymbol = defaultSymbol.ToUpperInvariant().Trim();
        DefaultTimeframe = defaultTimeframe;
        RiskPercent = riskPercent;
        StopLossPips = stopLossPips;
        TakeProfitPips = takeProfitPips;
        UseTrailingStop = useTrailingStop;
        TrailingStopPips = trailingStopPips;
        BreakEvenAtPipsEnabled = breakEvenAtPipsEnabled;
        BreakEvenTriggerPips = breakEvenTriggerPips;
        ParametersJson = parametersJson.Trim();
        IsActive = isActive;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
