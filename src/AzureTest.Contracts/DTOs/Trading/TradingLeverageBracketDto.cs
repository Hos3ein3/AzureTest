using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Trading;

public sealed record TradingBracketTierDto(
    int Bracket,
    int InitialLeverage,
    decimal NotionalCap,
    decimal NotionalFloor,
    decimal MaintMarginRatio,
    decimal Cum
);

public sealed record TradingLeverageBracketDto(
    string Symbol,
    int MaxLeverage,
    int MinLeverage,
    IReadOnlyList<TradingBracketTierDto> Brackets
);
