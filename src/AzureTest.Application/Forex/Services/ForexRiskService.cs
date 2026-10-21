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
        var propFirm = EvaluatePropFirmRules(account);
        var weekendGap = CheckWeekendGapRisk(openTrades);
        var swapCalendar = GetSwapRolloverSchedule();
        var killSwitch = GetKillSwitchStatus();

        return new ForexRiskOverviewDto(
            account,
            sampleSizing,
            currencyExposures,
            newsFilter,
            compliance,
            propFirm,
            weekendGap,
            swapCalendar,
            killSwitch
        );
    }

    public PropFirmEvaluationDto EvaluatePropFirmRules(ForexAccountDto account)
    {
        var dailyLossPct = account.DailyDrawdownPercent;
        var maxDailyAllowed = 5.0m;
        var dailyBreached = dailyLossPct >= maxDailyAllowed;

        var peakBal = account.PeakEquity > 0 ? account.PeakEquity : account.Balance;
        var maxDrawdownPct = peakBal > 0 ? Math.Round(((peakBal - account.Equity) / peakBal) * 100m, 2) : 0m;
        var maxDdAllowed = 10.0m;
        var maxDdBreached = maxDrawdownPct >= maxDdAllowed;

        var targetProfitPct = 10.0m;
        var currentProfitPct = account.Balance > 0 ? Math.Round(((account.Equity - 100_000m) / 100_000m) * 100m, 2) : 0m;
        var targetReached = currentProfitPct >= targetProfitPct;

        int minDays = 4;
        int daysTraded = 5;
        bool passed = !dailyBreached && !maxDdBreached && targetReached && daysTraded >= minDays;

        string summary = dailyBreached
            ? "FAILED: Daily loss limit breached (>= 5.0%)"
            : (maxDdBreached
                ? "FAILED: Maximum trailing drawdown breached (>= 10.0%)"
                : (passed ? "PASSED: Prop Firm Evaluation Criteria Met!" : "IN PROGRESS: Challenge Phase 1 active"));

        return new PropFirmEvaluationDto(
            "FTMO 100k Challenge (Phase 1)",
            100_000m,
            maxDailyAllowed,
            account.Balance * (dailyLossPct / 100m),
            dailyLossPct,
            !dailyBreached,
            maxDdAllowed,
            maxDrawdownPct,
            !maxDdBreached,
            targetProfitPct,
            currentProfitPct,
            targetReached,
            minDays,
            daysTraded,
            passed,
            summary
        );
    }

    public WeekendGapProtectionDto CheckWeekendGapRisk(IReadOnlyList<ForexTradeDto> openTrades)
    {
        var now = DateTime.UtcNow;
        var isFriday = now.DayOfWeek == DayOfWeek.Friday;
        var fridayCloseUtc = new TimeSpan(21, 0, 0); // 21:00 UTC / 5 PM NY Friday close
        var timeUntilClose = isFriday && now.TimeOfDay < fridayCloseUtc
            ? fridayCloseUtc - now.TimeOfDay
            : TimeSpan.Zero;

        var isApproaching = isFriday && now.TimeOfDay >= new TimeSpan(19, 0, 0); // within 2 hours of close
        var openCount = openTrades.Count;
        var totalNotional = openTrades.Sum(t => t.LotSize * 100_000m);

        return new WeekendGapProtectionDto(
            IsAutoFlattenEnabled: true,
            FridayFlattenTimeUtc: fridayCloseUtc,
            IsMarketCloseApproaching: isApproaching,
            TimeUntilFridayClose: timeUntilClose,
            OpenTradesAtRiskCount: openCount,
            TotalNotionalExposureAtRisk: totalNotional
        );
    }

    public SwapRolloverCalendarDto GetSwapRolloverSchedule()
    {
        var now = DateTime.UtcNow;
        var daysUntilWed = ((int)DayOfWeek.Wednesday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilWed == 0 && now.Hour >= 21) daysUntilWed = 7;
        var nextTripleSwap = now.Date.AddDays(daysUntilWed).AddHours(21); // 21:00 UTC rollover

        var swapRates = new List<PairSwapRateDto>
        {
            new("EUR/USD", -5.8m, +1.2m, -5.80m, +1.20m, now.DayOfWeek == DayOfWeek.Wednesday),
            new("GBP/USD", -4.2m, +0.6m, -4.20m, +0.60m, now.DayOfWeek == DayOfWeek.Wednesday),
            new("USD/JPY", +12.4m, -18.6m, +8.12m, -12.18m, now.DayOfWeek == DayOfWeek.Wednesday),
            new("AUD/USD", -2.8m, -0.4m, -2.80m, -0.40m, now.DayOfWeek == DayOfWeek.Wednesday),
            new("USD/CHF", +4.5m, -8.2m, +5.10m, -9.30m, now.DayOfWeek == DayOfWeek.Wednesday),
            new("XAU/USD", -14.2m, +2.5m, -14.20m, +2.50m, now.DayOfWeek == DayOfWeek.Wednesday)
        };

        return new SwapRolloverCalendarDto(
            "Wednesday (3x Financing Days Applied at 21:00 UTC)",
            nextTripleSwap,
            swapRates,
            -18.40m
        );
    }

    public KillSwitchStatusDto GetKillSwitchStatus()
    {
        // System wide risk state
        return new KillSwitchStatusDto(
            IsActive: false,
            ActivatedAtUtc: null,
            Reason: null,
            PositionsLiquidated: 0
        );
    }
}
