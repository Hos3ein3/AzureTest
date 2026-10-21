using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;

namespace AzureTest.Application.Crypto.Services;

public class CryptoAnalysisService : ICryptoAnalysisService
{
    private readonly ICryptoMarketDataService _marketData;

    public CryptoAnalysisService(ICryptoMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public CryptoTechnicalIndicatorsDto CalculateIndicators(string symbol, CryptoTimeframe timeframe, IReadOnlyList<CryptoCandleDto> candles)
    {
        if (candles == null || candles.Count == 0)
        {
            return new CryptoTechnicalIndicatorsDto(
                symbol, timeframe, 0, 0, 0, 0, 0, 0, 0, 50, "Neutral", 0, 0, 0, "Neutral", 0, 0, 0, 0, 0, 0, 0, "Neutral");
        }

        var closes = candles.Select(c => c.Close).ToList();
        var currentPrice = closes[^1];

        decimal ema9 = CalculateEma(closes, 9);
        decimal ema21 = CalculateEma(closes, 21);
        decimal ema50 = CalculateEma(closes, 50);
        decimal ema200 = CalculateEma(closes, 200);

        decimal sma20 = CalculateSma(closes, 20);
        decimal sma50 = CalculateSma(closes, 50);

        decimal rsi14 = CalculateRsi(closes, 14);
        string rsiSignal = rsi14 switch
        {
            >= 70 => "Overbought (High Pullback Risk)",
            <= 30 => "Oversold (Bounce Potential)",
            >= 55 => "Bullish Momentum",
            <= 45 => "Bearish Pressure",
            _ => "Neutral Range"
        };

        var (macdLine, signalLine, macdHist) = CalculateMacd(closes);
        string macdSignal = macdHist switch
        {
            > 0 and > 1.0m => "Strong Bullish Expansion",
            > 0 => "Bullish Convergence",
            < 0 and < -1.0m => "Strong Bearish Expansion",
            < 0 => "Bearish Convergence",
            _ => "Neutral / Flattening"
        };

        var (bbUpper, bbMiddle, bbLower, bbBandwidth) = CalculateBollingerBands(closes, 20, 2.0m);
        decimal atr14 = CalculateAtr(candles, 14);

        decimal vol24h = candles.TakeLast(24).Sum(c => c.Volume);
        decimal volMa = candles.Count >= 20 ? candles.TakeLast(20).Average(c => c.Volume) : vol24h;

        // Overall signal derivation
        int bullishScore = 0;
        int bearishScore = 0;

        if (currentPrice > ema21) bullishScore++; else bearishScore++;
        if (ema9 > ema21) bullishScore++; else bearishScore++;
        if (currentPrice > ema50) bullishScore++; else bearishScore++;
        if (rsi14 is > 50 and < 70) bullishScore++;
        else if (rsi14 is < 50 and > 30) bearishScore++;
        if (macdHist > 0) bullishScore++; else bearishScore++;
        if (currentPrice > bbMiddle) bullishScore++; else bearishScore++;

        string overallSignal = (bullishScore - bearishScore) switch
        {
            >= 4 => "Strong Buy",
            >= 2 => "Buy",
            <= -4 => "Strong Sell",
            <= -2 => "Sell",
            _ => "Neutral"
        };

        return new CryptoTechnicalIndicatorsDto(
            Symbol: symbol,
            Timeframe: timeframe,
            CurrentPrice: Math.Round(currentPrice, 4),
            Ema9: Math.Round(ema9, 4),
            Ema21: Math.Round(ema21, 4),
            Ema50: Math.Round(ema50, 4),
            Ema200: Math.Round(ema200, 4),
            Sma20: Math.Round(sma20, 4),
            Sma50: Math.Round(sma50, 4),
            Rsi14: Math.Round(rsi14, 2),
            RsiSignal: rsiSignal,
            MacdLine: Math.Round(macdLine, 4),
            SignalLine: Math.Round(signalLine, 4),
            MacdHistogram: Math.Round(macdHist, 4),
            MacdSignal: macdSignal,
            BollingerUpper: Math.Round(bbUpper, 4),
            BollingerMiddle: Math.Round(bbMiddle, 4),
            BollingerLower: Math.Round(bbLower, 4),
            BollingerBandwidth: Math.Round(bbBandwidth, 2),
            Atr14: Math.Round(atr14, 4),
            Volume24h: Math.Round(vol24h, 2),
            VolumeMa: Math.Round(volMa, 2),
            OverallSignal: overallSignal);
    }

    public CryptoSupportResistanceDto CalculateSupportResistance(IReadOnlyList<CryptoCandleDto> candles, decimal currentPrice)
    {
        if (candles == null || candles.Count == 0)
        {
            return new CryptoSupportResistanceDto(
                currentPrice,
                currentPrice * 1.02m,
                currentPrice * 1.04m,
                currentPrice * 1.06m,
                currentPrice * 0.98m,
                currentPrice * 0.96m,
                currentPrice * 0.94m,
                currentPrice,
                "Neutral",
                "Moderate",
                currentPrice * 1.01m,
                currentPrice * 1.005m,
                currentPrice,
                currentPrice * 0.995m,
                currentPrice * 0.99m);
        }

        var recent = candles.TakeLast(24).ToList();
        decimal high = recent.Max(c => c.High);
        decimal low = recent.Min(c => c.Low);
        decimal close = recent[^1].Close;

        decimal pivot = (high + low + close) / 3.0m;
        decimal r1 = (2m * pivot) - low;
        decimal s1 = (2m * pivot) - high;
        decimal r2 = pivot + (high - low);
        decimal s2 = pivot - (high - low);
        decimal r3 = high + (2m * (pivot - low));
        decimal s3 = low - (2m * (high - pivot));

        // Fibonacci retracement calculations based on recent range
        decimal diff = high - low;
        decimal fib236 = high - (diff * 0.236m);
        decimal fib382 = high - (diff * 0.382m);
        decimal fib500 = high - (diff * 0.500m);
        decimal fib618 = high - (diff * 0.618m);
        decimal fib786 = high - (diff * 0.786m);

        // Trend assessment
        decimal ema50 = CalculateEma(candles.Select(c => c.Close).ToList(), 50);
        decimal rsi = CalculateRsi(candles.Select(c => c.Close).ToList(), 14);

        string trend = (currentPrice > ema50 && rsi > 52) ? "Bullish" : (currentPrice < ema50 && rsi < 48) ? "Bearish" : "Consolidation";
        string strength = (rsi > 65 || rsi < 35) ? "Strong" : (rsi is > 55 or < 45) ? "Moderate" : "Weak";

        return new CryptoSupportResistanceDto(
            Pivot: Math.Round(pivot, 4),
            R1: Math.Round(r1, 4),
            R2: Math.Round(r2, 4),
            R3: Math.Round(r3, 4),
            S1: Math.Round(s1, 4),
            S2: Math.Round(s2, 4),
            S3: Math.Round(s3, 4),
            CurrentPrice: Math.Round(currentPrice, 4),
            TrendDirection: trend,
            TrendStrength: strength,
            Fibonacci236: Math.Round(fib236, 4),
            Fibonacci382: Math.Round(fib382, 4),
            Fibonacci500: Math.Round(fib500, 4),
            Fibonacci618: Math.Round(fib618, 4),
            Fibonacci786: Math.Round(fib786, 4));
    }

    public async Task<CryptoAnalysisDto> GetComprehensiveAnalysisAsync(string symbol, CryptoTimeframe timeframe, CancellationToken ct = default)
    {
        var candles = await _marketData.GetCandlesAsync(symbol, timeframe, 120, ct);
        var currentPrice = candles.Count > 0 ? candles[^1].Close : await _marketData.GetCurrentPriceAsync(symbol, ct);

        var technical = CalculateIndicators(symbol, timeframe, candles);
        var sr = CalculateSupportResistance(candles, currentPrice);
        var sentiment = await _marketData.GetSentimentAsync(ct);
        var whaleAlerts = await _marketData.GetWhaleAlertsAsync(ct);
        var screener = await RunScreenerAsync(ct);

        return new CryptoAnalysisDto(technical, sr, sentiment, whaleAlerts, screener);
    }

    public async Task<IReadOnlyList<CryptoScreenerItemDto>> RunScreenerAsync(CancellationToken ct = default)
    {
        var overview = await _marketData.GetMarketOverviewAsync(new CryptoMarketFilter { PageSize = 25 }, ct);
        var list = new List<CryptoScreenerItemDto>();

        foreach (var coin in overview.Coins.Take(15))
        {
            var candles = await _marketData.GetCandlesAsync(coin.Symbol, CryptoTimeframe.H1, 60, ct);
            var indicators = CalculateIndicators(coin.Symbol, CryptoTimeframe.H1, candles);

            decimal volatility = coin.Price > 0 ? Math.Round((indicators.Atr14 / coin.Price) * 100m, 2) : 2.5m;
            decimal momentumScore = Math.Round((indicators.Rsi14 * 0.5m) + (coin.Change24h * 1.5m), 1);

            list.Add(new CryptoScreenerItemDto(
                Rank: coin.Rank,
                Symbol: coin.Symbol,
                Name: coin.Name,
                Price: coin.Price,
                Change24h: coin.Change24h,
                Volume24h: coin.Volume24h,
                VolatilityPercent: volatility,
                Rsi: indicators.Rsi14,
                MacdStatus: indicators.MacdSignal,
                MomentumScore: momentumScore,
                Recommendation: indicators.OverallSignal));
        }

        return list.OrderByDescending(x => x.MomentumScore).ToList();
    }

    // ── Indicator Computation Algorithms ────────────────────────────────────────

    private static decimal CalculateSma(List<decimal> data, int period)
    {
        if (data.Count == 0) return 0m;
        if (data.Count < period) return data.Average();
        return data.TakeLast(period).Average();
    }

    private static decimal CalculateEma(List<decimal> data, int period)
    {
        if (data.Count == 0) return 0m;
        if (data.Count < period) return data.Average();

        decimal multiplier = 2.0m / (period + 1);
        decimal ema = data.Take(period).Average();

        for (int i = period; i < data.Count; i++)
        {
            ema = (data[i] * multiplier) + (ema * (1.0m - multiplier));
        }

        return ema;
    }

    private static decimal CalculateRsi(List<decimal> data, int period)
    {
        if (data.Count <= period) return 50m;

        decimal gainSum = 0m;
        decimal lossSum = 0m;

        for (int i = 1; i <= period; i++)
        {
            decimal diff = data[i] - data[i - 1];
            if (diff >= 0) gainSum += diff;
            else lossSum += Math.Abs(diff);
        }

        decimal avgGain = gainSum / period;
        decimal avgLoss = lossSum / period;

        for (int i = period + 1; i < data.Count; i++)
        {
            decimal diff = data[i] - data[i - 1];
            decimal gain = diff >= 0 ? diff : 0m;
            decimal loss = diff < 0 ? Math.Abs(diff) : 0m;

            avgGain = ((avgGain * (period - 1)) + gain) / period;
            avgLoss = ((avgLoss * (period - 1)) + loss) / period;
        }

        if (avgLoss == 0m) return 100m;
        decimal rs = avgGain / avgLoss;
        return 100m - (100m / (1m + rs));
    }

    private static (decimal MacdLine, decimal SignalLine, decimal Histogram) CalculateMacd(List<decimal> data)
    {
        if (data.Count < 26) return (0m, 0m, 0m);

        var macdSeries = new List<decimal>();
        for (int i = 26; i <= data.Count; i++)
        {
            var slice = data.Take(i).ToList();
            decimal fast = CalculateEma(slice, 12);
            decimal slow = CalculateEma(slice, 26);
            macdSeries.Add(fast - slow);
        }

        decimal macdLine = macdSeries[^1];
        decimal signalLine = macdSeries.Count >= 9 ? CalculateEma(macdSeries, 9) : macdLine;
        decimal histogram = macdLine - signalLine;

        return (macdLine, signalLine, histogram);
    }

    private static (decimal Upper, decimal Middle, decimal Lower, decimal Bandwidth) CalculateBollingerBands(List<decimal> data, int period, decimal stdDevMultiplier)
    {
        if (data.Count < period)
        {
            decimal p = data.Count > 0 ? data[^1] : 0m;
            return (p * 1.02m, p, p * 0.98m, 4.0m);
        }

        var slice = data.TakeLast(period).ToList();
        decimal middle = slice.Average();

        decimal sumSquares = slice.Sum(x => (x - middle) * (x - middle));
        decimal stdDev = (decimal)Math.Sqrt((double)(sumSquares / period));

        decimal upper = middle + (stdDevMultiplier * stdDev);
        decimal lower = Math.Max(0.0001m, middle - (stdDevMultiplier * stdDev));
        decimal bandwidth = middle > 0 ? ((upper - lower) / middle) * 100m : 0m;

        return (upper, middle, lower, bandwidth);
    }

    private static decimal CalculateAtr(IReadOnlyList<CryptoCandleDto> candles, int period)
    {
        if (candles.Count <= 1) return 0m;

        var trs = new List<decimal>();
        for (int i = 1; i < candles.Count; i++)
        {
            decimal highLow = candles[i].High - candles[i].Low;
            decimal highPrevClose = Math.Abs(candles[i].High - candles[i - 1].Close);
            decimal lowPrevClose = Math.Abs(candles[i].Low - candles[i - 1].Close);
            trs.Add(Math.Max(highLow, Math.Max(highPrevClose, lowPrevClose)));
        }

        if (trs.Count < period) return trs.Average();
        decimal atr = trs.Take(period).Average();
        for (int i = period; i < trs.Count; i++)
        {
            atr = ((atr * (period - 1)) + trs[i]) / period;
        }

        return atr;
    }
}
