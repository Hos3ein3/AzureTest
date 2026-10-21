using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Forex;

public sealed class ForexApiSetting : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int KeyMaxLength = 200;
        public const int ProviderMaxLength = 50;
        public const int SimulatedDelayMsMin = 0;
        public const int SimulatedDelayMsMax = 10000;
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string TwelveDataApiKey { get; private set; } = string.Empty;
    public string AlphaVantageApiKey { get; private set; } = string.Empty;
    public string FredApiKey { get; private set; } = string.Empty;
    public string FinnhubApiKey { get; private set; } = string.Empty;
    public string OandaApiKey { get; private set; } = string.Empty;
    public string OandaAccountId { get; private set; } = string.Empty;
    public string PrimaryPriceProvider { get; private set; } = "Frankfurter"; // Default free no key provider!
    public bool UseSimulatedExecutionBroker { get; private set; } = true;
    public int SimulatedLatencyMs { get; private set; } = 150;
    public bool EnableSafeRetries { get; private set; } = true;
    public bool EnableNewsFilterWarning { get; private set; } = true;

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private ForexApiSetting() { }

    private ForexApiSetting(
        Guid userId,
        string twelveDataApiKey,
        string alphaVantageApiKey,
        string fredApiKey,
        string finnhubApiKey,
        string oandaApiKey,
        string oandaAccountId,
        string primaryPriceProvider,
        bool useSimulatedExecutionBroker,
        int simulatedLatencyMs,
        bool enableSafeRetries,
        bool enableNewsFilterWarning,
        Guid? tenantId = null)
    {
        UserId = userId;
        TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId;
        TwelveDataApiKey = twelveDataApiKey?.Trim() ?? string.Empty;
        AlphaVantageApiKey = alphaVantageApiKey?.Trim() ?? string.Empty;
        FredApiKey = fredApiKey?.Trim() ?? string.Empty;
        FinnhubApiKey = finnhubApiKey?.Trim() ?? string.Empty;
        OandaApiKey = oandaApiKey?.Trim() ?? string.Empty;
        OandaAccountId = oandaAccountId?.Trim() ?? string.Empty;
        PrimaryPriceProvider = string.IsNullOrWhiteSpace(primaryPriceProvider) ? "Frankfurter" : primaryPriceProvider.Trim();
        UseSimulatedExecutionBroker = useSimulatedExecutionBroker;
        SimulatedLatencyMs = Math.Clamp(simulatedLatencyMs, Rules.SimulatedDelayMsMin, Rules.SimulatedDelayMsMax);
        EnableSafeRetries = enableSafeRetries;
        EnableNewsFilterWarning = enableNewsFilterWarning;
    }

    public static ForexApiSetting Create(
        Guid userId,
        string twelveDataApiKey = "",
        string alphaVantageApiKey = "",
        string fredApiKey = "",
        string finnhubApiKey = "",
        string oandaApiKey = "",
        string oandaAccountId = "",
        string primaryPriceProvider = "Frankfurter",
        bool useSimulatedExecutionBroker = true,
        int simulatedLatencyMs = 150,
        bool enableSafeRetries = true,
        bool enableNewsFilterWarning = true,
        Guid? tenantId = null)
    {
        return new ForexApiSetting(
            userId, twelveDataApiKey, alphaVantageApiKey, fredApiKey, finnhubApiKey,
            oandaApiKey, oandaAccountId, primaryPriceProvider, useSimulatedExecutionBroker,
            simulatedLatencyMs, enableSafeRetries, enableNewsFilterWarning, tenantId);
    }

    public void UpdateKeys(
        string twelveDataApiKey,
        string alphaVantageApiKey,
        string fredApiKey,
        string finnhubApiKey,
        string oandaApiKey,
        string oandaAccountId,
        string primaryPriceProvider,
        bool useSimulatedExecutionBroker,
        int simulatedLatencyMs,
        bool enableSafeRetries,
        bool enableNewsFilterWarning)
    {
        TwelveDataApiKey = twelveDataApiKey?.Trim() ?? string.Empty;
        AlphaVantageApiKey = alphaVantageApiKey?.Trim() ?? string.Empty;
        FredApiKey = fredApiKey?.Trim() ?? string.Empty;
        FinnhubApiKey = finnhubApiKey?.Trim() ?? string.Empty;
        OandaApiKey = oandaApiKey?.Trim() ?? string.Empty;
        OandaAccountId = oandaAccountId?.Trim() ?? string.Empty;
        PrimaryPriceProvider = string.IsNullOrWhiteSpace(primaryPriceProvider) ? "Frankfurter" : primaryPriceProvider.Trim();
        UseSimulatedExecutionBroker = useSimulatedExecutionBroker;
        SimulatedLatencyMs = Math.Clamp(simulatedLatencyMs, Rules.SimulatedDelayMsMin, Rules.SimulatedDelayMsMax);
        EnableSafeRetries = enableSafeRetries;
        EnableNewsFilterWarning = enableNewsFilterWarning;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
