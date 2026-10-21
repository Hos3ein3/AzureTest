using System;
using System.Collections.Generic;
using System.Linq;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Forex.Services;

public sealed class ForexRiskService : IForexRiskService
{
    private readonly ILogger<ForexRiskService> _logger;

    public ForexRiskService(ILogger<ForexRiskService> logger)
    {
        _logger = logger;
    }

    public LotSizeCalculationResultDto CalculateLotSize(LotSizeCalculationRequestDto request)
    {
        if (request.AccountBalance <= 0 || request.StopLossPips <= 0 || request.RiskPercent <= 0)
        {
            return new LotSizeCalculationResultDto(0.01m, 0m, 0m, 0m, 0m, 0m, false, "Invalid parameters");
        }

        var symbol = request.Symbol.ToUpperInvariant().Trim();
        var isJpy = symbol.Contains("JPY", StringComparison.OrdinalIgnoreCase);
        var isGold = symbol.Contains("XAU", StringComparison.OrdinalIgnoreCase);

        // Pip value for 1.0 standard lot ($100,000 notional)
        // EUR/USD = $10.00 / pip
        // USD/JPY = ~$6.50 / pip
        // XAU/USD = $10.00 / pip (0.10 move = $10)
        decimal pipValuePerStandardLot = isJpy ? 6.55m : 10.00m;

        decimal amountAtRisk = Math.Round(request.AccountBalance * (request.RiskPercent / 100m), 2);
        decimal unroundedLotSize = amountAtRisk / (request.StopLossPips * pipValuePerStandardLot);
        decimal recommendedLotSize = Math.Round(unroundedLotSize, 2);
        recommendedLotSize = Math.Clamp(recommendedLotSize, 0.01m, 50.00m);

        decimal standardContractUnits = isGold ? 100m : 100_000m;
        decimal positionValue = Math.Round(recommendedLotSize * standardContractUnits, 2);

        // ESMA max leverage: 1:30 for major forex (3.33% margin), 1:20 for minor forex and gold (5.00% margin)
        bool isMajor = symbol is "EUR/USD" or "GBP/USD" or "USD/JPY" or "USD/CHF" or "AUD/USD" or "USD/CAD" or "NZD/USD";
        int esmaMaxLev = isMajor ? 30 : 20;

        decimal marginPercentage = 100m / esmaMaxLev; // 3.33% or 5.0%
        decimal requiredMargin = Math.Round(positionValue * (marginPercentage / 100m), 2);
        decimal actualLeverageUsed = request.AccountBalance > 0 ? Math.Round(positionValue / request.AccountBalance, 1) : 0m;

        bool exceedsEsma = actualLeverageUsed > esmaMaxLev;
        string warningMessage = exceedsEsma
            ? $"WARNING: Position leverage ({actualLeverageUsed}:1) exceeds the ESMA/AFM retail cap of {esmaMaxLev}:1. Reduce lot size to stay compliant."
            : "Risk parameters comply with ESMA/AFM guidelines.";

        return new LotSizeCalculationResultDto(
            recommendedLotSize,
            amountAtRisk,
            Math.Round(recommendedLotSize * pipValuePerStandardLot, 2),
            positionValue,
            requiredMargin,
            actualLeverageUsed,
            exceedsEsma,
            warningMessage
        );
    }

    public NewsRiskFilterDto CheckNewsTradeFilter(IReadOnlyList<EconomicEventDto> upcomingEvents, int filterWindowMinutes = 30)
    {
        var now = DateTime.UtcNow;
        var highImpactUpcoming = upcomingEvents
            .Where(e => e.Impact == ForexImpactLevel.High)
            .Where(e => Math.Abs((e.EventTimeUtc - now).TotalMinutes) <= filterWindowMinutes)
            .OrderBy(e => e.EventTimeUtc)
            .ToList();

        if (highImpactUpcoming.Count > 0)
        {
            var nearest = highImpactUpcoming.First();
            var mins = (int)Math.Round((nearest.EventTimeUtc - now).TotalMinutes);
            var status = mins >= 0
                ? $"RESTRICTED: High-impact economic news '{nearest.Title}' ({nearest.Currency}) in {mins} minutes. No new entries recommended."
                : $"RESTRICTED: High-impact economic news '{nearest.Title}' was released {Math.Abs(mins)} minutes ago. High spread & slippage risk.";

            return new NewsRiskFilterDto(false, status, highImpactUpcoming, Math.Abs(mins));
        }

        return new NewsRiskFilterDto(true, "CLEAR: No high-impact news within the +/- 30-minute risk window.", highImpactUpcoming, 999);
    }

    public IReadOnlyList<CurrencyExposureDto> CalculateCurrencyExposures(IReadOnlyList<ForexTradeDto> openTrades, decimal accountEquity)
    {
        var currencies = new Dictionary<string, (decimal Long, decimal Short)>(StringComparer.OrdinalIgnoreCase);

        foreach (var trade in openTrades.Where(t => t.Status == ForexTradeStatus.Open))
        {
            var parts = trade.Symbol.Split('/');
            if (parts.Length == 2)
            {
                var baseCurr = parts[0];
                var quoteCurr = parts[1];

                if (!currencies.ContainsKey(baseCurr)) currencies[baseCurr] = (0m, 0m);
                if (!currencies.ContainsKey(quoteCurr)) currencies[quoteCurr] = (0m, 0m);

                if (trade.Direction == ForexTradeDirection.Buy)
                {
                    currencies[baseCurr] = (currencies[baseCurr].Long + trade.LotSize, currencies[baseCurr].Short);
                    currencies[quoteCurr] = (currencies[quoteCurr].Long, currencies[quoteCurr].Short + trade.LotSize);
                }
                else
                {
                    currencies[baseCurr] = (currencies[baseCurr].Long, currencies[baseCurr].Short + trade.LotSize);
                    currencies[quoteCurr] = (currencies[quoteCurr].Long + trade.LotSize, currencies[quoteCurr].Short);
                }
            }
        }

        return currencies.Select(kvp =>
        {
            var net = kvp.Value.Long - kvp.Value.Short;
            var notional = Math.Abs(net) * 100_000m;
            var pct = accountEquity > 0 ? Math.Round((notional / accountEquity) * 100m, 1) : 0m;
            var isOverexposed = pct > 300m; // over 3x equity on single currency

            return new CurrencyExposureDto(kvp.Key, kvp.Value.Long, kvp.Value.Short, net, pct, isOverexposed);
        }).OrderByDescending(x => Math.Abs(x.NetExposureLots)).ToList();
    }

    public EsmaAfmComplianceDto GetRegulatoryComplianceProfile()
    {
        return new EsmaAfmComplianceDto(
            MaxPermittedLeverage: 30, // 1:30 for major FX pairs
            MaxMarginUsageAllowed: 50.0m, // 50% margin stop-out mandatory under ESMA
            NegativeBalanceProtectionGuaranteed: true, // Retail account cannot go into debt
            BinaryOptionsProhibited: true, // AFM strict prohibition
            RegulatoryFramework: "ESMA MiFID II / AFM Netherlands (Wft)",
            Country: "Netherlands (EU)"
        );
    }

    public ForexRiskOverviewDto GetRiskOverview(ForexAccountDto account, IReadOnlyList<ForexTradeDto> openTrades, IReadOnlyList<EconomicEventDto> upcomingEvents)
    {
        var sampleSizing = CalculateLotSize(new LotSizeCalculationRequestDto(account.Equity, 1.0m, 25.0m, "EUR/USD", 30m));
        var newsFilter = CheckNewsTradeFilter(upcomingEvents);
        var currencyExposures = CalculateCurrencyExposures(openTrades, account.Equity);
        var compliance = GetRegulatoryComplianceProfile();

        return new ForexRiskOverviewDto(account, sampleSizing, currencyExposures, newsFilter, compliance);
    }
}
