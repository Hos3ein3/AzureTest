namespace AzureTest.Contracts.DTOs.Forex;

public sealed record ForexApiSettingsDto(
    string TwelveDataApiKey,
    string AlphaVantageApiKey,
    string FredApiKey,
    string FinnhubApiKey,
    string OandaApiKey,
    string OandaAccountId,
    string PrimaryPriceProvider,
    bool UseSimulatedExecutionBroker,
    int SimulatedLatencyMs,
    bool EnableSafeRetries,
    bool EnableNewsFilterWarning
);
