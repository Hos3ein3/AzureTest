using System;
using AzureTest.Domain.Entities.Forex.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Forex;

public sealed class ForexEconomicAlert : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int EventNameMinLength = 2;
        public const int EventNameMaxLength = 150;
        public const int CurrencyMaxLength = 10;
        public const int LeadMinutesMin = 1;
        public const int LeadMinutesMax = 1440; // up to 24h
        public const int NoteMaxLength = 500;
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string EventName { get; private set; } = string.Empty;
    public string Currency { get; private set; } = "USD";
    public ForexImpactLevel ImpactLevel { get; private set; } = ForexImpactLevel.High;
    public int LeadMinutes { get; private set; } = 30;
    public bool AutoNoTradeFilterEnabled { get; private set; } = true;
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private ForexEconomicAlert() { }

    private ForexEconomicAlert(
        Guid userId,
        string eventName,
        string currency,
        ForexImpactLevel impactLevel,
        int leadMinutes,
        bool autoNoTradeFilterEnabled,
        string? notes = null,
        Guid? tenantId = null)
    {
        UserId = userId;
        TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId;
        EventName = eventName.Trim();
        Currency = currency.ToUpperInvariant().Trim();
        ImpactLevel = impactLevel;
        LeadMinutes = leadMinutes;
        AutoNoTradeFilterEnabled = autoNoTradeFilterEnabled;
        Notes = notes?.Trim();
        IsActive = true;
    }

    public static ForexEconomicAlert Create(
        Guid userId,
        string eventName,
        string currency,
        ForexImpactLevel impactLevel,
        int leadMinutes = 30,
        bool autoNoTradeFilterEnabled = true,
        string? notes = null,
        Guid? tenantId = null)
    {
        return new ForexEconomicAlert(userId, eventName, currency, impactLevel, leadMinutes, autoNoTradeFilterEnabled, notes, tenantId);
    }

    public void ToggleActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
