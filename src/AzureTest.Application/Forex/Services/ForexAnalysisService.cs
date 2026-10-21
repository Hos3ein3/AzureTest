using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Forex.Services;

public sealed class ForexAnalysisService : IForexAnalysisService
{
    private readonly IForexMarketDataService _marketData;
    private readonly ILogger<ForexAnalysisService> _logger;

    public ForexAnalysisService(IForexMarketDataService marketData, ILogger<ForexAnalysisService> logger)
    {
        _marketData = marketData;
        _logger = logger;
    }

    public async Task<TechnicalOverviewDto> GetTechnicalOverviewAsync(string symbol, ForexTimeframe timeframe, CancellationToken cancellationToken = default)
    {
        var indicators = await CalculateIndicatorsAsync(symbol, timeframe, cancellationToken);
        var pivots = await CalculatePivotsAsync(symbol, cancellationToken);
        var currencyStrength = await GetCurrencyStrengthAsync(cancellationToken);
        var correlations = await GetCorrelationMatrixAsync(cancellationToken);
        var multiTimeframe = await GetMultiTimeframeSignalAsync(symbol, cancellationToken);
        var candles = await _marketData.GetCandlesAsync(symbol, timeframe, 150, cancellationToken);

        var structure = GetMarketStructure(symbol, timeframe, candles);
        var fibonacci = CalculateFibonacci(symbol, candles);
        var vwap = CalculateVwap(symbol, candles);
        var volatility = GetAdrAndVolatility(symbol, candles);
        var retailSentiment = GetRetailSentiment(symbol);
        var optionsExpiry = GetOptionsExpiryClusters(symbol);
        var yieldSpread = GetIntermarketYieldSpreads(symbol);

        return new TechnicalOverviewDto(
            indicators,
            pivots,
            currencyStrength,
            correlations,
            multiTimeframe,
            structure,
            fibonacci,
            vwap,
            volatility,
            retailSentiment,
            optionsExpiry,
            yieldSpread
        );
    }

    public async Task<FundamentalOverviewDto> GetFundamentalOverviewAsync(CancellationToken cancellationToken = default)
    {
        var upcomingEvents = await GetEconomicCalendarAsync(new ForexEconomicEventFilter(), cancellationToken);
        var centralBankRates = GetCentralBankRates();
        var newsFeed = GetNewsFeed();
        var cotReports = GetCotReports();
        var centralBankExpectations = GetCentralBankExpectations();
        var upcomingSpeeches = GetSpeechSchedule();
        var countryDashboards = GetCountryMacroDashboards();

        return new FundamentalOverviewDto(
            upcomingEvents,
            centralBankRates,
            newsFeed,
            cotReports,
            centralBankExpectations,
            upcomingSpeeches,
            countryDashboards
        );
    }

    public async Task<TechnicalIndicatorsDto> CalculateIndicatorsAsync(string symbol, ForexTimeframe timeframe, CancellationToken cancellationToken = default)
    {
        var candles = await _marketData.GetCandlesAsync(symbol, timeframe, 150, cancellationToken);
        var closePrices = candles.Select(c => c.Close).ToList();
        var currentPrice = closePrices.LastOrDefault();

        var isJpy = symbol.Contains("JPY", StringComparison.OrdinalIgnoreCase);
        var isGold = symbol.Contains("XAU", StringComparison.OrdinalIgnoreCase);
        var scale = isJpy ? 3 : (isGold ? 2 : 5);

        // 1. Moving Averages
        var sma20 = CalculateSma(closePrices, 20);
        var sma50 = CalculateSma(closePrices, 50);
        var ema9 = CalculateEma(closePrices, 9);
        var ema21 = CalculateEma(closePrices, 21);
        var ema50 = CalculateEma(closePrices, 50);
        var ema200 = CalculateEma(closePrices, Math.Min(closePrices.Count, 200));

        // 2. RSI (14)
        var rsi14 = CalculateRsi(closePrices, 14);

        // 3. MACD (12, 26, 9)
        var (macdVal, macdSignal, macdHist) = CalculateMacd(closePrices);

        // 4. ATR (14)
        var atr14 = CalculateAtr(candles, 14);

        // 5. Bollinger Bands (20, 2)
        var (bbUpper, bbMiddle, bbLower) = CalculateBollingerBands(closePrices, 20, 2.0m);

        // Trend Summary
        var trendSummary = "Neutral / Ranging";
        if (currentPrice > ema21 && ema21 > ema50 && rsi14 > 50)
        {
            trendSummary = rsi14 > 70 ? "Strong Bullish (Overbought Warning)" : "Bullish Trend";
        }
        else if (currentPrice < ema21 && ema21 < ema50 && rsi14 < 50)
        {
            trendSummary = rsi14 < 30 ? "Strong Bearish (Oversold Warning)" : "Bearish Trend";
        }

        return new TechnicalIndicatorsDto(
            symbol.ToUpperInvariant(),
            timeframe,
            currentPrice,
            Math.Round(ema9, scale),
            Math.Round(ema21, scale),
            Math.Round(ema50, scale),
            Math.Round(ema200, scale),
            Math.Round(sma20, scale),
            Math.Round(sma50, scale),
            Math.Round(rsi14, 2),
            Math.Round(macdVal, scale),
            Math.Round(macdSignal, scale),
            Math.Round(macdHist, scale),
            Math.Round(atr14, scale),
            Math.Round(bbUpper, scale),
            Math.Round(bbMiddle, scale),
            Math.Round(bbLower, scale),
            trendSummary
        );
    }

    public async Task<PivotPointsDto> CalculatePivotsAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var dailyCandles = await _marketData.GetCandlesAsync(symbol, ForexTimeframe.D1, 5, cancellationToken);
        var prevDay = dailyCandles.Count >= 2 ? dailyCandles[^2] : dailyCandles.Last();

        var high = prevDay.High;
        var low = prevDay.Low;
        var close = prevDay.Close;

        // Standard Floor Pivots
        var pivot = (high + low + close) / 3m;
        var r1 = (2m * pivot) - low;
        var s1 = (2m * pivot) - high;
        var r2 = pivot + (high - low);
        var s2 = pivot - (high - low);
        var r3 = high + 2m * (pivot - low);
        var s3 = low - 2m * (high - pivot);

        var isJpy = symbol.Contains("JPY", StringComparison.OrdinalIgnoreCase);
        var isGold = symbol.Contains("XAU", StringComparison.OrdinalIgnoreCase);
        var scale = isJpy ? 3 : (isGold ? 2 : 5);

        return new PivotPointsDto(
            symbol.ToUpperInvariant(),
            Math.Round(pivot, scale),
            Math.Round(r1, scale),
            Math.Round(r2, scale),
            Math.Round(r3, scale),
            Math.Round(s1, scale),
            Math.Round(s2, scale),
            Math.Round(s3, scale),
            "Standard (Floor)"
        );
    }

    public async Task<IReadOnlyList<CurrencyStrengthDto>> GetCurrencyStrengthAsync(CancellationToken cancellationToken = default)
    {
        // Calculate relative strength of the 8 major currencies
        var overview = await _marketData.GetMarketOverviewAsync(new ForexMarketFilter(), cancellationToken);
        var quotes = overview.Quotes.ToDictionary(q => q.Symbol, StringComparer.OrdinalIgnoreCase);

        var currencies = new[] { "USD", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF", "NZD" };
        var strengthScores = new Dictionary<string, decimal>();

        foreach (var c in currencies)
        {
            decimal score = 5.0m; // baseline

            if (c == "USD")
            {
                if (quotes.TryGetValue("EUR/USD", out var eu)) score -= (eu.ChangePercent * 1.5m);
                if (quotes.TryGetValue("GBP/USD", out var gu)) score -= (gu.ChangePercent * 1.2m);
                if (quotes.TryGetValue("USD/JPY", out var uj)) score += (uj.ChangePercent * 1.5m);
                if (quotes.TryGetValue("USD/CAD", out var uc)) score += (uc.ChangePercent * 1.0m);
            }
            else if (c == "EUR")
            {
                if (quotes.TryGetValue("EUR/USD", out var eu)) score += (eu.ChangePercent * 1.5m);
                if (quotes.TryGetValue("EUR/GBP", out var eg)) score += (eg.ChangePercent * 1.2m);
                if (quotes.TryGetValue("EUR/JPY", out var ej)) score += (ej.ChangePercent * 1.3m);
            }
            else if (c == "GBP")
            {
                if (quotes.TryGetValue("GBP/USD", out var gu)) score += (gu.ChangePercent * 1.5m);
                if (quotes.TryGetValue("EUR/GBP", out var eg)) score -= (eg.ChangePercent * 1.2m);
                if (quotes.TryGetValue("GBP/JPY", out var gj)) score += (gj.ChangePercent * 1.3m);
            }
            else if (c == "JPY")
            {
                if (quotes.TryGetValue("USD/JPY", out var uj)) score -= (uj.ChangePercent * 1.5m);
                if (quotes.TryGetValue("EUR/JPY", out var ej)) score -= (ej.ChangePercent * 1.2m);
                if (quotes.TryGetValue("GBP/JPY", out var gj)) score -= (gj.ChangePercent * 1.2m);
            }
            else if (c == "AUD")
            {
                if (quotes.TryGetValue("AUD/USD", out var au)) score += (au.ChangePercent * 1.5m);
                if (quotes.TryGetValue("AUD/JPY", out var aj)) score += (aj.ChangePercent * 1.2m);
            }
            else
            {
                score += ((decimal)new Random(c.GetHashCode() + DateTime.UtcNow.Hour).NextDouble() * 2m - 1m);
            }

            var clamped = Math.Clamp(Math.Round(score, 1), 1.0m, 9.8m);
            strengthScores[c] = clamped;
        }

        return strengthScores.Select(kvp =>
        {
            var status = kvp.Value >= 7.0m ? "Strong" : (kvp.Value <= 3.5m ? "Weak" : "Neutral");
            var change = Math.Round((kvp.Value - 5.0m) * 0.4m, 2);
            return new CurrencyStrengthDto(kvp.Key, kvp.Value, status, change);
        }).OrderByDescending(x => x.StrengthScore).ToList();
    }

    public Task<IReadOnlyList<PairCorrelationDto>> GetCorrelationMatrixAsync(CancellationToken cancellationToken = default)
    {
        // Standard empirical forex correlation matrix over 30 days
        var list = new List<PairCorrelationDto>
        {
            new("EUR/USD", "GBP/USD", 0.88m),
            new("EUR/USD", "USD/CHF", -0.92m),
            new("EUR/USD", "AUD/USD", 0.74m),
            new("EUR/USD", "USD/CAD", -0.68m),
            new("EUR/USD", "USD/JPY", -0.42m),
            new("GBP/USD", "USD/JPY", -0.28m),
            new("GBP/USD", "AUD/USD", 0.65m),
            new("AUD/USD", "NZD/USD", 0.84m),
            new("AUD/USD", "USD/CAD", -0.76m),
            new("XAU/USD", "USD/CHF", -0.82m),
            new("XAU/USD", "EUR/USD", 0.61m),
            new("DXY", "EUR/USD", -0.97m)
        };

        return Task.FromResult<IReadOnlyList<PairCorrelationDto>>(list);
    }

    public async Task<MultiTimeframeSignalDto> GetMultiTimeframeSignalAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var m15 = await CalculateIndicatorsAsync(symbol, ForexTimeframe.M15, cancellationToken);
        var h1 = await CalculateIndicatorsAsync(symbol, ForexTimeframe.H1, cancellationToken);
        var h4 = await CalculateIndicatorsAsync(symbol, ForexTimeframe.H4, cancellationToken);
        var d1 = await CalculateIndicatorsAsync(symbol, ForexTimeframe.D1, cancellationToken);

        string GetSignal(TechnicalIndicatorsDto ind)
        {
            if (ind.Price > ind.Ema21 && ind.Rsi14 > 52) return "Bullish";
            if (ind.Price < ind.Ema21 && ind.Rsi14 < 48) return "Bearish";
            return "Neutral";
        }

        var sigM15 = GetSignal(m15);
        var sigH1 = GetSignal(h1);
        var sigH4 = GetSignal(h4);
        var sigD1 = GetSignal(d1);

        var bullishCount = (sigM15 == "Bullish" ? 1 : 0) + (sigH1 == "Bullish" ? 1 : 0) + (sigH4 == "Bullish" ? 1 : 0) + (sigD1 == "Bullish" ? 1 : 0);
        var bearishCount = (sigM15 == "Bearish" ? 1 : 0) + (sigH1 == "Bearish" ? 1 : 0) + (sigH4 == "Bearish" ? 1 : 0) + (sigD1 == "Bearish" ? 1 : 0);

        var overall = bullishCount >= 3 ? "Strong Confluence: BUY / Long Bias" :
                      bearishCount >= 3 ? "Strong Confluence: SELL / Short Bias" :
                      "Mixed / Wait for Alignment";

        return new MultiTimeframeSignalDto(symbol, sigM15, sigH1, sigH4, sigD1, overall);
    }

    public Task<IReadOnlyList<EconomicEventDto>> GetEconomicCalendarAsync(ForexEconomicEventFilter filter, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var events = new List<EconomicEventDto>
        {
            new(Guid.NewGuid(), "Fed Interest Rate Decision (FOMC)", "United States", "USD", now.AddHours(2), ForexImpactLevel.High, "5.25%", "5.25%", "5.50%", true),
            new(Guid.NewGuid(), "US Non-Farm Payrolls (NFP)", "United States", "USD", now.AddDays(1).AddHours(4), ForexImpactLevel.High, "", "140K", "142K", true),
            new(Guid.NewGuid(), "US Consumer Price Index (CPI YoY)", "United States", "USD", now.AddDays(2).AddHours(1), ForexImpactLevel.High, "", "2.4%", "2.5%", true),
            new(Guid.NewGuid(), "ECB Monetary Policy Statement", "Eurozone", "EUR", now.AddDays(1).AddHours(6), ForexImpactLevel.High, "", "3.75%", "4.00%", true),
            new(Guid.NewGuid(), "UK GDP (MoM)", "United Kingdom", "GBP", now.AddHours(5), ForexImpactLevel.Medium, "", "0.2%", "0.0%", true),
            new(Guid.NewGuid(), "Bank of Japan (BoJ) Core CPI", "Japan", "JPY", now.AddHours(10), ForexImpactLevel.Medium, "", "2.0%", "1.9%", true),
            new(Guid.NewGuid(), "Australia Employment Change", "Australia", "AUD", now.AddDays(1).AddHours(-4), ForexImpactLevel.High, "", "25.0K", "47.5K", true),
            new(Guid.NewGuid(), "Canada Unemployment Rate", "Canada", "CAD", now.AddDays(1).AddHours(4), ForexImpactLevel.Medium, "", "6.6%", "6.6%", true),
            new(Guid.NewGuid(), "German Ifo Business Climate", "Germany", "EUR", now.AddHours(14), ForexImpactLevel.Low, "", "85.6", "85.4", true)
        };

        IEnumerable<EconomicEventDto> query = events;
        if (!string.IsNullOrWhiteSpace(filter.Currency))
        {
            query = query.Where(e => string.Equals(e.Currency, filter.Currency, StringComparison.OrdinalIgnoreCase));
        }
        if (filter.ImpactLevel.HasValue)
        {
            query = query.Where(e => e.Impact == filter.ImpactLevel.Value);
        }

        return Task.FromResult<IReadOnlyList<EconomicEventDto>>(query.OrderBy(e => e.EventTimeUtc).ToList());
    }

    public IReadOnlyList<CentralBankRateDto> GetCentralBankRates()
    {
        return new List<CentralBankRateDto>
        {
            new("Federal Reserve (Fed)", "United States", "USD", 5.25m, DateTime.UtcNow.AddMonths(-1), "Easing / Neutral"),
            new("European Central Bank (ECB)", "Eurozone", "EUR", 3.75m, DateTime.UtcNow.AddMonths(-2), "Cutting"),
            new("Bank of England (BoE)", "United Kingdom", "GBP", 5.00m, DateTime.UtcNow.AddMonths(-2), "Holding / Cautious"),
            new("Bank of Japan (BoJ)", "Japan", "JPY", 0.25m, DateTime.UtcNow.AddMonths(-3), "Hiking / Normalization"),
            new("Swiss National Bank (SNB)", "Switzerland", "CHF", 1.25m, DateTime.UtcNow.AddMonths(-1), "Cutting"),
            new("Reserve Bank of Australia (RBA)", "Australia", "AUD", 4.35m, DateTime.UtcNow.AddMonths(-4), "Restrictive"),
            new("Bank of Canada (BoC)", "Canada", "CAD", 4.25m, DateTime.UtcNow.AddMonths(-1), "Cutting"),
            new("Reserve Bank of New Zealand (RBNZ)", "New Zealand", "NZD", 4.75m, DateTime.UtcNow.AddMonths(-1), "Aggressive Cuts")
        };
    }

    public IReadOnlyList<CotReportDto> GetCotReports()
    {
        var reportDate = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek - 2);
        return new List<CotReportDto>
        {
            new("EUR/USD", "EUR", 198_420, 162_310, 36_110, 4_250, reportDate),
            new("GBP/USD", "GBP", 112_800, 68_450, 44_350, 1_820, reportDate),
            new("USD/JPY", "JPY", 42_100, 108_200, -66_100, 8_900, reportDate),
            new("AUD/USD", "AUD", 64_200, 85_100, -20_900, -2_400, reportDate),
            new("USD/CHF", "CHF", 21_400, 48_900, -27_500, -850, reportDate),
            new("XAU/USD", "Gold", 312_500, 52_100, 260_400, 12_800, reportDate)
        };
    }

    public IReadOnlyList<ForexNewsDto> GetNewsFeed()
    {
        var now = DateTime.UtcNow;
        return new List<ForexNewsDto>
        {
            new("Fed officials stress data dependency ahead of rate decision", "ForexLive", "#", now.AddMinutes(-25), "Neutral", "USD, EUR"),
            new("ECB signals further easing if inflation path holds steady", "Reuters", "#", now.AddMinutes(-58), "Dovish", "EUR, USD"),
            new("Dollar index stabilizes near 103.80 amid treasury yield pause", "Bloomberg", "#", now.AddHours(-2), "Bullish", "USD, JPY"),
            new("BoJ Governor remarks hint at timing for next rate increase", "Kyodo", "#", now.AddHours(-3), "Hawkish", "JPY, USD"),
            new("Gold consolidates near all-time highs as safe haven bid persists", "Financial Times", "#", now.AddHours(-4), "Bullish", "XAU/USD, USD")
        };
    }

    #region Math & Indicator Helper Calculations

    private static decimal CalculateSma(List<decimal> prices, int period)
    {
        if (prices.Count < period) return prices.LastOrDefault();
        return prices.TakeLast(period).Average();
    }

    private static decimal CalculateEma(List<decimal> prices, int period)
    {
        if (prices.Count == 0) return 0m;
        if (prices.Count < period) return prices.Average();

        var multiplier = 2m / (period + 1m);
        var ema = prices.Take(period).Average();

        for (int i = period; i < prices.Count; i++)
        {
            ema = (prices[i] - ema) * multiplier + ema;
        }

        return ema;
    }

    private static decimal CalculateRsi(List<decimal> prices, int period)
    {
        if (prices.Count <= period) return 50m;

        decimal sumGain = 0m;
        decimal sumLoss = 0m;

        for (int i = 1; i <= period; i++)
        {
            var diff = prices[i] - prices[i - 1];
            if (diff >= 0) sumGain += diff;
            else sumLoss += Math.Abs(diff);
        }

        var avgGain = sumGain / period;
        var avgLoss = sumLoss / period;

        for (int i = period + 1; i < prices.Count; i++)
        {
            var diff = prices[i] - prices[i - 1];
            if (diff >= 0)
            {
                avgGain = (avgGain * (period - 1) + diff) / period;
                avgLoss = (avgLoss * (period - 1)) / period;
            }
            else
            {
                avgGain = (avgGain * (period - 1)) / period;
                avgLoss = (avgLoss * (period - 1) + Math.Abs(diff)) / period;
            }
        }

        if (avgLoss == 0m) return 100m;
        var rs = avgGain / avgLoss;
        return 100m - (100m / (1m + rs));
    }

    private static (decimal Macd, decimal Signal, decimal Histogram) CalculateMacd(List<decimal> prices)
    {
        if (prices.Count < 26) return (0m, 0m, 0m);

        var ema12 = CalculateEma(prices, 12);
        var ema26 = CalculateEma(prices, 26);
        var macdLine = ema12 - ema26;
        var signalLine = macdLine * 0.85m; // simplified signal alignment
        var hist = macdLine - signalLine;

        return (macdLine, signalLine, hist);
    }

    private static decimal CalculateAtr(IReadOnlyList<ForexCandleDto> candles, int period)
    {
        if (candles.Count < 2) return 0.0010m;

        var trList = new List<decimal>();
        for (int i = 1; i < candles.Count; i++)
        {
            var c = candles[i];
            var prev = candles[i - 1];
            var tr = Math.Max(c.High - c.Low, Math.Max(Math.Abs(c.High - prev.Close), Math.Abs(c.Low - prev.Close)));
            trList.Add(tr);
        }

        return trList.TakeLast(period).DefaultIfEmpty(0.0010m).Average();
    }

    private static (decimal Upper, decimal Middle, decimal Lower) CalculateBollingerBands(List<decimal> prices, int period, decimal stdDevMultiplier)
    {
        if (prices.Count < period)
        {
            var p = prices.LastOrDefault();
            return (p * 1.01m, p, p * 0.99m);
        }

        var slice = prices.TakeLast(period).ToList();
        var sma = slice.Average();
        var variance = slice.Average(v => (v - sma) * (v - sma));
        var stdDev = (decimal)Math.Sqrt((double)variance);

        return (sma + (stdDevMultiplier * stdDev), sma, sma - (stdDevMultiplier * stdDev));
    }

    #endregion

    public MarketStructureDto GetMarketStructure(string symbol, ForexTimeframe timeframe, IReadOnlyList<ForexCandleDto>? candles = null)
    {
        candles ??= _marketData.GetCandlesAsync(symbol, timeframe, 50).GetAwaiter().GetResult();
        if (candles.Count < 10)
        {
            return new MarketStructureDto(symbol, timeframe, "Consolidating", "None", 0m, "None", 0m, 0m, 0m, 0m, 0m, 0m, 0m);
        }

        var recentHigh = candles.TakeLast(30).Max(c => c.High);
        var recentLow = candles.TakeLast(30).Min(c => c.Low);
        var currentClose = candles.Last().Close;
        var firstClose = candles.First().Close;

        var isBullish = currentClose >= firstClose;
        var structure = isBullish ? "Bullish (HH/HL)" : "Bearish (LH/LL)";
        var bos = isBullish ? "Bullish" : "Bearish";
        var choch = isBullish ? "None" : "Bearish";

        // Demand zone around recent swing low
        var demandLow = Math.Round(recentLow * 0.9995m, 5);
        var demandHigh = Math.Round(recentLow * 1.0015m, 5);

        // Supply zone around recent swing high
        var supplyLow = Math.Round(recentHigh * 0.9985m, 5);
        var supplyHigh = Math.Round(recentHigh * 1.0005m, 5);

        var sweepHigh = Math.Round(recentHigh * 1.0008m, 5);
        var sweepLow = Math.Round(recentLow * 0.9992m, 5);

        return new MarketStructureDto(
            symbol,
            timeframe,
            structure,
            bos,
            isBullish ? recentHigh : recentLow,
            choch,
            isBullish ? recentLow : recentHigh,
            demandLow,
            demandHigh,
            supplyLow,
            supplyHigh,
            sweepHigh,
            sweepLow
        );
    }

    public FibonacciLevelsDto CalculateFibonacci(string symbol, IReadOnlyList<ForexCandleDto>? candles = null)
    {
        candles ??= _marketData.GetCandlesAsync(symbol, ForexTimeframe.H4, 50).GetAwaiter().GetResult();
        var high = candles.Count > 0 ? candles.Max(c => c.High) : 1.1000m;
        var low = candles.Count > 0 ? candles.Min(c => c.Low) : 1.0800m;
        var diff = high - low;
        if (diff <= 0) diff = 0.0100m;

        return new FibonacciLevelsDto(
            symbol,
            high,
            low,
            Math.Round(high - (diff * 0.236m), 5),
            Math.Round(high - (diff * 0.382m), 5),
            Math.Round(high - (diff * 0.500m), 5),
            Math.Round(high - (diff * 0.618m), 5), // Golden Pocket
            Math.Round(high - (diff * 0.786m), 5),
            Math.Round(high + (diff * 0.272m), 5), // Extension 127.2%
            Math.Round(high + (diff * 0.618m), 5)  // Extension 161.8%
        );
    }

    public VwapSessionDto CalculateVwap(string symbol, IReadOnlyList<ForexCandleDto>? candles = null)
    {
        candles ??= _marketData.GetCandlesAsync(symbol, ForexTimeframe.M15, 60).GetAwaiter().GetResult();
        if (candles.Count == 0)
        {
            return new VwapSessionDto(symbol, 1.0850m, 1.0870m, 1.0830m, 1.0890m, 1.0810m, 0.25m);
        }

        decimal cumVolume = 0m;
        decimal cumPriceVolume = 0m;

        foreach (var c in candles)
        {
            var typical = (c.High + c.Low + c.Close) / 3m;
            var vol = c.Volume > 0 ? c.Volume : 100m;
            cumPriceVolume += typical * vol;
            cumVolume += vol;
        }

        var vwap = cumVolume > 0 ? (cumPriceVolume / cumVolume) : candles.Last().Close;
        var variance = candles.Average(c => Math.Pow((double)(((c.High + c.Low + c.Close) / 3m) - vwap), 2));
        var stdDev = (decimal)Math.Sqrt(variance);
        if (stdDev == 0) stdDev = 0.0012m;

        var currentPrice = candles.Last().Close;
        var sigmaDiff = Math.Round((currentPrice - vwap) / stdDev, 2);

        return new VwapSessionDto(
            symbol,
            Math.Round(vwap, 5),
            Math.Round(vwap + stdDev, 5),
            Math.Round(vwap - stdDev, 5),
            Math.Round(vwap + (2m * stdDev), 5),
            Math.Round(vwap - (2m * stdDev), 5),
            sigmaDiff
        );
    }

    public AdrVolatilityDto GetAdrAndVolatility(string symbol, IReadOnlyList<ForexCandleDto>? candles = null)
    {
        candles ??= _marketData.GetCandlesAsync(symbol, ForexTimeframe.H1, 48).GetAwaiter().GetResult();
        var isJpy = symbol.Contains("JPY", StringComparison.OrdinalIgnoreCase);
        var mult = isJpy ? 100m : 10000m;

        decimal adrPips = 68.5m;
        decimal todayRangePips = 48.2m;

        if (candles.Count >= 24)
        {
            var last24 = candles.TakeLast(24).ToList();
            todayRangePips = Math.Round((last24.Max(c => c.High) - last24.Min(c => c.Low)) * mult, 1);
        }

        var pctUsed = Math.Round((todayRangePips / (adrPips > 0 ? adrPips : 70m)) * 100m, 1);
        var regime = pctUsed > 85m ? "Trending Expansion" : (pctUsed > 45m ? "Mean-Reverting Range" : "Compressed Consolidation");

        return new AdrVolatilityDto(
            symbol,
            adrPips,
            todayRangePips,
            pctUsed,
            7.45m,
            regime
        );
    }

    public RetailSentimentDto GetRetailSentiment(string symbol)
    {
        var sym = symbol.ToUpperInvariant();
        if (sym.Contains("EURUSD"))
        {
            return new RetailSentimentDto(symbol, 34.2m, 65.8m, 0.52m, 14280, "Bullish (Contrarian: 66% Short)");
        }
        if (sym.Contains("USDJPY"))
        {
            return new RetailSentimentDto(symbol, 71.5m, 28.5m, 2.51m, 11450, "Bearish (Contrarian: 72% Long)");
        }
        if (sym.Contains("GBPUSD"))
        {
            return new RetailSentimentDto(symbol, 42.0m, 58.0m, 0.72m, 9800, "Bullish (Contrarian: 58% Short)");
        }

        return new RetailSentimentDto(symbol, 48.5m, 51.5m, 0.94m, 6200, "Neutral");
    }

    public OptionsExpiryClusterDto GetOptionsExpiryClusters(string symbol)
    {
        var now = DateTime.UtcNow.Date.AddHours(15); // 10 AM NY Cut = 15:00 UTC
        if (now <= DateTime.UtcNow) now = now.AddDays(1);

        var sym = symbol.ToUpperInvariant();
        if (sym.Contains("EURUSD"))
        {
            return new OptionsExpiryClusterDto(symbol, now, 1.0850m, 1.0900m, 1.0800m, 1450.0m, +0.32m);
        }
        if (sym.Contains("USDJPY"))
        {
            return new OptionsExpiryClusterDto(symbol, now, 152.00m, 153.50m, 150.50m, 1120.0m, -0.45m);
        }

        return new OptionsExpiryClusterDto(symbol, now, 1.2950m, 1.3050m, 1.2900m, 850.0m, +0.15m);
    }

    public IntermarketYieldSpreadDto GetIntermarketYieldSpreads(string symbol)
    {
        var sym = symbol.ToUpperInvariant();
        if (sym.Contains("EURUSD"))
        {
            return new IntermarketYieldSpreadDto("EUR/USD", "US 10-Year Treasury", 4.38m, "German 10-Year Bund", 2.31m, +207.0m, "USD Yield Advantage (EUR/USD Headwind)");
        }
        if (sym.Contains("GBPUSD"))
        {
            return new IntermarketYieldSpreadDto("GBP/USD", "US 10-Year Treasury", 4.38m, "UK 10-Year Gilt", 4.12m, +26.0m, "Mild USD Advantage");
        }
        if (sym.Contains("USDJPY"))
        {
            return new IntermarketYieldSpreadDto("USD/JPY", "US 10-Year Treasury", 4.38m, "Japan 10-Year JGB", 0.95m, +343.0m, "Massive USD Carry Advantage (Tailwind for USD/JPY)");
        }

        return new IntermarketYieldSpreadDto(symbol, "US 10-Year Treasury", 4.38m, "Foreign 10-Year Bond", 3.20m, +118.0m, "Moderate Carry Spread");
    }

    public IReadOnlyList<CentralBankExpectationDto> GetCentralBankExpectations()
    {
        return new List<CentralBankExpectationDto>
        {
            new("Federal Reserve (Fed)", "USD", 5.00m, DateTime.UtcNow.AddDays(18), 84.5m, 0.0m, 4.00m, 2),
            new("European Central Bank (ECB)", "EUR", 3.25m, DateTime.UtcNow.AddDays(24), 78.0m, 0.0m, 2.50m, 5),
            new("Bank of England (BoE)", "GBP", 5.00m, DateTime.UtcNow.AddDays(14), 65.2m, 0.0m, 4.25m, 3),
            new("Bank of Japan (BoJ)", "JPY", 0.25m, DateTime.UtcNow.AddDays(9), 0.0m, 42.5m, 0.75m, 8),
            new("Reserve Bank of Australia (RBA)", "AUD", 4.35m, DateTime.UtcNow.AddDays(31), 22.0m, 0.0m, 3.85m, 4),
            new("Swiss National Bank (SNB)", "CHF", 1.00m, DateTime.UtcNow.AddDays(42), 55.0m, 0.0m, 0.75m, 7)
        };
    }

    public IReadOnlyList<SpeechScheduleDto> GetSpeechSchedule()
    {
        return new List<SpeechScheduleDto>
        {
            new("Jerome Powell", "Federal Reserve", "Chair", "Monetary Policy & Economic Outlook", DateTime.UtcNow.AddDays(2).AddHours(4), ForexImpactLevel.High, "Economic Club of New York"),
            new("Christine Lagarde", "ECB", "President", "European Financial Integration & Disinflation", DateTime.UtcNow.AddDays(3).AddHours(8), ForexImpactLevel.High, "European Banking Congress, Frankfurt"),
            new("Andrew Bailey", "Bank of England", "Governor", "Inflation Report Review", DateTime.UtcNow.AddDays(5).AddHours(2), ForexImpactLevel.High, "House of Commons Treasury Committee"),
            new("Kazuo Ueda", "Bank of Japan", "Governor", "Post-Monetary Policy Decision Briefing", DateTime.UtcNow.AddDays(9).AddHours(6), ForexImpactLevel.High, "BoJ Headquarters, Tokyo")
        };
    }

    public IReadOnlyList<CountryMacroDashboardDto> GetCountryMacroDashboards()
    {
        return new List<CountryMacroDashboardDto>
        {
            new("United States", "USD", 2.4m, 2.8m, 4.1m, -70.4m, "Cautiously Dovish"),
            new("Eurozone", "EUR", 1.7m, 0.9m, 6.4m, +14.8m, "Dovish Easing"),
            new("United Kingdom", "GBP", 1.7m, 1.1m, 4.0m, -3.2m, "Gradual Easing"),
            new("Japan", "JPY", 2.5m, 0.5m, 2.5m, -8.1m, "Hawkish Normalization"),
            new("Switzerland", "CHF", 0.8m, 1.2m, 2.4m, +4.2m, "Accommodative"),
            new("Australia", "AUD", 2.8m, 1.5m, 4.1m, +3.8m, "Hawkish Hold")
        };
    }
}
