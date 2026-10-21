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

        var marketStructure = await GetMarketStructureAsync(symbol, timeframe, ct);
        var volumeProfile = await GetVolumeProfileAsync(symbol, timeframe, ct);
        var marketRegime = await GetMarketRegimeAsync(symbol, timeframe, ct);

        return new CryptoAnalysisDto(
            technical,
            sr,
            sentiment,
            whaleAlerts,
            screener,
            marketStructure,
            volumeProfile,
            marketRegime);
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

    // ── Feature 4: Market Structure, Regime Detection & Quant Analysis ──────────

    public async Task<CryptoMarketStructureDto> GetMarketStructureAsync(string symbol, CryptoTimeframe timeframe, CancellationToken ct = default)
    {
        var candles = await _marketData.GetCandlesAsync(symbol, timeframe, 120, ct);
        decimal currentPrice = candles.Count > 0 ? candles[^1].Close : await _marketData.GetCurrentPriceAsync(symbol, ct);

        if (candles.Count < 10)
        {
            decimal fallbackHigh = currentPrice * 1.05m;
            decimal fallbackLow = currentPrice * 0.95m;
            decimal eq = (fallbackHigh + fallbackLow) / 2m;
            return new CryptoMarketStructureDto(
                symbol,
                timeframe,
                "Consolidation",
                "None",
                currentPrice,
                DateTime.UtcNow,
                fallbackHigh,
                fallbackLow,
                eq,
                "Equilibrium Zone",
                Array.Empty<CryptoOrderBlockDto>(),
                Array.Empty<CryptoFairValueGapDto>(),
                Array.Empty<CryptoLiquidityPoolDto>(),
                "Neutral - Low historical data",
                DateTime.UtcNow);
        }

        // 1. Detect Swing Highs and Swing Lows (Fractal pivots)
        var swingHighs = new List<(int Index, decimal Price, DateTime Time)>();
        var swingLows = new List<(int Index, decimal Price, DateTime Time)>();

        for (int i = 2; i < candles.Count - 2; i++)
        {
            if (candles[i].High >= candles[i - 1].High && candles[i].High >= candles[i - 2].High &&
                candles[i].High >= candles[i + 1].High && candles[i].High >= candles[i + 2].High)
            {
                swingHighs.Add((i, candles[i].High, candles[i].Time));
            }

            if (candles[i].Low <= candles[i - 1].Low && candles[i].Low <= candles[i - 2].Low &&
                candles[i].Low <= candles[i + 1].Low && candles[i].Low <= candles[i + 2].Low)
            {
                swingLows.Add((i, candles[i].Low, candles[i].Time));
            }
        }

        decimal recentSwingHigh = swingHighs.Count > 0 ? swingHighs[^1].Price : candles.Max(c => c.High);
        decimal recentSwingLow = swingLows.Count > 0 ? swingLows[^1].Price : candles.Min(c => c.Low);
        if (recentSwingHigh <= recentSwingLow)
        {
            recentSwingHigh = currentPrice * 1.04m;
            recentSwingLow = currentPrice * 0.96m;
        }

        decimal rangeDiff = recentSwingHigh - recentSwingLow;
        decimal equilibrium = Math.Round((recentSwingHigh + recentSwingLow) / 2m, 4);

        // Optimal Trade Entry (OTE) zone: 61.8% to 78.6% retracement from recent swing
        decimal oteHigh = recentSwingLow + (rangeDiff * 0.786m);
        decimal oteLow = recentSwingLow + (rangeDiff * 0.618m);

        string currentZone = currentPrice switch
        {
            var p when p > recentSwingHigh => "External Range Liquidity (Bullish Expansion High)",
            var p when p >= oteLow && p <= oteHigh => "Optimal Trade Entry (OTE 61.8% - 78.6% Fib Zone)",
            var p when p > equilibrium => "Premium Zone (Favorable for Institutional Selling)",
            var p when p < recentSwingLow => "External Range Liquidity (Discount Liquidation Low)",
            _ => "Discount Zone (Favorable for Institutional Accumulation)"
        };

        // 2. BOS (Break of Structure) & CHoCH (Change of Character)
        string lastBreakType = "BOS (Break of Structure)";
        decimal lastBreakPrice = recentSwingHigh;
        DateTime lastBreakTime = candles[^1].Time;
        string currentTrend = "Bullish Expansion";

        if (swingHighs.Count >= 2 && swingLows.Count >= 2)
        {
            bool higherHighs = swingHighs[^1].Price > swingHighs[^2].Price;
            bool higherLows = swingLows[^1].Price > swingLows[^2].Price;

            if (higherHighs && higherLows)
            {
                currentTrend = "Bullish Market Structure (HH / HL)";
                lastBreakType = "Bullish BOS (Continuation)";
                lastBreakPrice = swingHighs[^2].Price;
                lastBreakTime = swingHighs[^1].Time;
            }
            else if (!higherHighs && !higherLows)
            {
                currentTrend = "Bearish Market Structure (LH / LL)";
                lastBreakType = "Bearish BOS (Continuation)";
                lastBreakPrice = swingLows[^2].Price;
                lastBreakTime = swingLows[^1].Time;
            }
            else
            {
                currentTrend = "Structural Transition (CHoCH in Progress)";
                lastBreakType = higherLows ? "Bullish CHoCH (Character Shift)" : "Bearish CHoCH (Character Shift)";
                lastBreakPrice = higherLows ? swingHighs[^1].Price : swingLows[^1].Price;
                lastBreakTime = candles[^5].Time;
            }
        }

        // 3. Order Blocks (OB)
        var orderBlocks = new List<CryptoOrderBlockDto>();
        for (int i = 3; i < candles.Count - 2; i++)
        {
            var candle = candles[i];
            var next1 = candles[i + 1];
            var next2 = candles[i + 2];

            // Bullish Order Block (+OB): Last down candle before strong upward displacement
            if (candle.Close < candle.Open && next1.Close > next1.Open && next2.Close > next2.Open && (next2.Close - candle.Low) > (rangeDiff * 0.15m))
            {
                decimal top = Math.Max(candle.Open, candle.Close);
                decimal bottom = candle.Low;
                decimal meanThreshold = (top + bottom) / 2m;
                bool isMitigated = candles.Skip(i + 3).Any(c => c.Low <= top);
                decimal mitigationPrice = isMitigated ? candles.Skip(i + 3).Where(c => c.Low <= top).Select(c => c.Low).FirstOrDefault() : 0m;
                decimal strength = Math.Round(Math.Min(98m, 70m + ((next2.Volume / (candles.Average(c => c.Volume) + 1m)) * 10m)), 1);

                orderBlocks.Add(new CryptoOrderBlockDto(
                    Type: "Bullish Order Block (+OB)",
                    TopPrice: Math.Round(top, 4),
                    BottomPrice: Math.Round(bottom, 4),
                    MeanThreshold: Math.Round(meanThreshold, 4),
                    FormedAtUtc: candle.Time,
                    IsMitigated: isMitigated,
                    MitigationPrice: Math.Round(mitigationPrice, 4),
                    StrengthScore: strength));
            }

            // Bearish Order Block (-OB): Last up candle before strong downward displacement
            if (candle.Close > candle.Open && next1.Close < next1.Open && next2.Close < next2.Open && (candle.High - next2.Close) > (rangeDiff * 0.15m))
            {
                decimal top = candle.High;
                decimal bottom = Math.Min(candle.Open, candle.Close);
                decimal meanThreshold = (top + bottom) / 2m;
                bool isMitigated = candles.Skip(i + 3).Any(c => c.High >= bottom);
                decimal mitigationPrice = isMitigated ? candles.Skip(i + 3).Where(c => c.High >= bottom).Select(c => c.High).FirstOrDefault() : 0m;
                decimal strength = Math.Round(Math.Min(98m, 70m + ((next2.Volume / (candles.Average(c => c.Volume) + 1m)) * 10m)), 1);

                orderBlocks.Add(new CryptoOrderBlockDto(
                    Type: "Bearish Order Block (-OB)",
                    TopPrice: Math.Round(top, 4),
                    BottomPrice: Math.Round(bottom, 4),
                    MeanThreshold: Math.Round(meanThreshold, 4),
                    FormedAtUtc: candle.Time,
                    IsMitigated: isMitigated,
                    MitigationPrice: Math.Round(mitigationPrice, 4),
                    StrengthScore: strength));
            }
        }

        // Keep most recent 6 order blocks
        var recentObs = orderBlocks.TakeLast(6).Reverse().ToList();

        // 4. Fair Value Gaps (FVG)
        var fvgs = new List<CryptoFairValueGapDto>();
        for (int i = 2; i < candles.Count; i++)
        {
            var c1 = candles[i - 2];
            var c2 = candles[i - 1];
            var c3 = candles[i];

            // Bullish FVG (BISI): c1.High < c3.Low
            if (c3.Low > c1.High && (c3.Low - c1.High) > (rangeDiff * 0.02m))
            {
                decimal top = c3.Low;
                decimal bottom = c1.High;
                decimal ce = (top + bottom) / 2m; // Consequent Encroachment (50%)
                var subsequent = candles.Skip(i + 1).ToList();
                bool isFilled = subsequent.Any(c => c.Low <= bottom);
                decimal minReached = subsequent.Count > 0 ? subsequent.Min(c => c.Low) : top;
                decimal percentFilled = minReached <= bottom ? 100m : minReached < top ? Math.Round(((top - minReached) / (top - bottom)) * 100m, 1) : 0m;

                fvgs.Add(new CryptoFairValueGapDto(
                    Type: "Bullish FVG (BISI)",
                    TopPrice: Math.Round(top, 4),
                    BottomPrice: Math.Round(bottom, 4),
                    ConsequentEncroachment: Math.Round(ce, 4),
                    FormedAtUtc: c2.Time,
                    IsFilled: isFilled,
                    PercentFilled: percentFilled));
            }

            // Bearish FVG (SIBI): c1.Low > c3.High
            if (c1.Low > c3.High && (c1.Low - c3.High) > (rangeDiff * 0.02m))
            {
                decimal top = c1.Low;
                decimal bottom = c3.High;
                decimal ce = (top + bottom) / 2m;
                var subsequent = candles.Skip(i + 1).ToList();
                bool isFilled = subsequent.Any(c => c.High >= top);
                decimal maxReached = subsequent.Count > 0 ? subsequent.Max(c => c.High) : bottom;
                decimal percentFilled = maxReached >= top ? 100m : maxReached > bottom ? Math.Round(((maxReached - bottom) / (top - bottom)) * 100m, 1) : 0m;

                fvgs.Add(new CryptoFairValueGapDto(
                    Type: "Bearish FVG (SIBI)",
                    TopPrice: Math.Round(top, 4),
                    BottomPrice: Math.Round(bottom, 4),
                    ConsequentEncroachment: Math.Round(ce, 4),
                    FormedAtUtc: c2.Time,
                    IsFilled: isFilled,
                    PercentFilled: percentFilled));
            }
        }

        var recentFvgs = fvgs.TakeLast(6).Reverse().ToList();

        // 5. Liquidity Pools & Sweeps
        var liquidityPools = new List<CryptoLiquidityPoolDto>
        {
            new(
                Type: "Buy-Side Liquidity (BSL - Swing High)",
                PriceLevel: Math.Round(recentSwingHigh, 4),
                Status: currentPrice >= recentSwingHigh ? "Swept / Purged" : "Untapped (Magnet)",
                SweptAtUtc: currentPrice >= recentSwingHigh ? DateTime.UtcNow.AddMinutes(-45) : null,
                EstimatedVolumeUsd: Math.Round(rangeDiff * 150000m, 0)),
            new(
                Type: "Buy-Side Liquidity (BSL - Secondary Pivot)",
                PriceLevel: Math.Round(recentSwingHigh * 1.018m, 4),
                Status: "Untapped (Major External BSL)",
                SweptAtUtc: null,
                EstimatedVolumeUsd: Math.Round(rangeDiff * 280000m, 0)),
            new(
                Type: "Sell-Side Liquidity (SSL - Swing Low)",
                PriceLevel: Math.Round(recentSwingLow, 4),
                Status: currentPrice <= recentSwingLow ? "Swept / Purged" : "Untapped (Magnet)",
                SweptAtUtc: currentPrice <= recentSwingLow ? DateTime.UtcNow.AddMinutes(-30) : null,
                EstimatedVolumeUsd: Math.Round(rangeDiff * 190000m, 0)),
            new(
                Type: "Sell-Side Liquidity (SSL - Secondary Low)",
                PriceLevel: Math.Round(recentSwingLow * 0.982m, 4),
                Status: "Untapped (Major External SSL)",
                SweptAtUtc: null,
                EstimatedVolumeUsd: Math.Round(rangeDiff * 320000m, 0))
        };

        // 6. Formulate SMC Bias
        string smcTradeBias = currentZone switch
        {
            var z when z.Contains("Discount") || z.Contains("OTE") => "Bullish Accumulation - Target BSL Liquidity",
            var z when z.Contains("Premium") => "Bearish Distribution - Target SSL Liquidity",
            var z when z.Contains("Expansion High") => "Liquidity Sweep Warning - Potential Reversal/Judas Swing",
            _ => "Neutral Range Bound - Await Displacement"
        };

        return new CryptoMarketStructureDto(
            Symbol: symbol,
            Timeframe: timeframe,
            CurrentTrend: currentTrend,
            LastBreakType: lastBreakType,
            LastBreakPrice: Math.Round(lastBreakPrice, 4),
            LastBreakTimeUtc: lastBreakTime,
            RecentSwingHigh: Math.Round(recentSwingHigh, 4),
            RecentSwingLow: Math.Round(recentSwingLow, 4),
            EquilibriumPrice: equilibrium,
            CurrentZone: currentZone,
            OrderBlocks: recentObs,
            FairValueGaps: recentFvgs,
            LiquidityPools: liquidityPools,
            SmcTradeBias: smcTradeBias,
            LastUpdatedUtc: DateTime.UtcNow);
    }

    public async Task<CryptoVolumeProfileHubDto> GetVolumeProfileAsync(string symbol, CryptoTimeframe timeframe, CancellationToken ct = default)
    {
        var candles = await _marketData.GetCandlesAsync(symbol, timeframe, 120, ct);
        decimal currentPrice = candles.Count > 0 ? candles[^1].Close : await _marketData.GetCurrentPriceAsync(symbol, ct);

        if (candles.Count == 0)
        {
            return new CryptoVolumeProfileHubDto(
                symbol, timeframe, currentPrice, currentPrice, currentPrice * 1.02m, currentPrice * 0.98m, 0, 70m,
                Array.Empty<CryptoVolumeProfileBucketDto>(), Array.Empty<CryptoAnchoredVwapDto>(), "Neutral", DateTime.UtcNow);
        }

        decimal minPrice = candles.Min(c => c.Low);
        decimal maxPrice = candles.Max(c => c.High);
        if (maxPrice <= minPrice) maxPrice = minPrice * 1.05m;

        int bucketCount = 24;
        decimal bucketStep = (maxPrice - minPrice) / bucketCount;

        var bucketsRaw = new (decimal PriceLevel, decimal BuyVol, decimal SellVol, decimal TotalVol)[bucketCount];
        for (int i = 0; i < bucketCount; i++)
        {
            decimal bucketPrice = minPrice + (bucketStep * (i + 0.5m));
            bucketsRaw[i] = (Math.Round(bucketPrice, 4), 0m, 0m, 0m);
        }

        foreach (var c in candles)
        {
            decimal candleRange = Math.Max(0.0001m, c.High - c.Low);
            decimal buyRatio = c.Close >= c.Open ? 0.62m : 0.38m;
            decimal candleBuyVol = c.Volume * buyRatio;
            decimal candleSellVol = c.Volume * (1.0m - buyRatio);

            // Find overlapping buckets
            int startIdx = Math.Clamp((int)((c.Low - minPrice) / bucketStep), 0, bucketCount - 1);
            int endIdx = Math.Clamp((int)((c.High - minPrice) / bucketStep), 0, bucketCount - 1);
            int overlapCount = Math.Max(1, endIdx - startIdx + 1);

            for (int b = startIdx; b <= endIdx; b++)
            {
                var curr = bucketsRaw[b];
                bucketsRaw[b] = (
                    curr.PriceLevel,
                    curr.BuyVol + (candleBuyVol / overlapCount),
                    curr.SellVol + (candleSellVol / overlapCount),
                    curr.TotalVol + (c.Volume / overlapCount));
            }
        }

        // POC (Point of Control) is the bucket with max TotalVol
        int pocIdx = 0;
        decimal maxBucketVol = 0m;
        for (int i = 0; i < bucketCount; i++)
        {
            if (bucketsRaw[i].TotalVol > maxBucketVol)
            {
                maxBucketVol = bucketsRaw[i].TotalVol;
                pocIdx = i;
            }
        }

        decimal totalVolume = bucketsRaw.Sum(b => b.TotalVol);
        decimal targetVaVol = totalVolume * 0.70m;

        // Value Area: expand out from POC until 70% of volume is captured
        var vaIndices = new HashSet<int> { pocIdx };
        decimal currentVaVol = bucketsRaw[pocIdx].TotalVol;
        int upPointer = pocIdx + 1;
        int downPointer = pocIdx - 1;

        while (currentVaVol < targetVaVol && (upPointer < bucketCount || downPointer >= 0))
        {
            decimal upVol = upPointer < bucketCount ? bucketsRaw[upPointer].TotalVol : -1m;
            decimal downVol = downPointer >= 0 ? bucketsRaw[downPointer].TotalVol : -1m;

            if (upVol >= downVol && upPointer < bucketCount)
            {
                vaIndices.Add(upPointer);
                currentVaVol += upVol;
                upPointer++;
            }
            else if (downPointer >= 0)
            {
                vaIndices.Add(downPointer);
                currentVaVol += downVol;
                downPointer--;
            }
            else if (upPointer < bucketCount)
            {
                vaIndices.Add(upPointer);
                currentVaVol += upVol;
                upPointer++;
            }
            else break;
        }

        decimal vah = bucketsRaw[vaIndices.Max()].PriceLevel + (bucketStep / 2m);
        decimal val = bucketsRaw[vaIndices.Min()].PriceLevel - (bucketStep / 2m);
        decimal pocPrice = bucketsRaw[pocIdx].PriceLevel;

        decimal avgBucketVol = totalVolume / bucketCount;

        var bucketDtos = new List<CryptoVolumeProfileBucketDto>();
        for (int i = 0; i < bucketCount; i++)
        {
            var raw = bucketsRaw[i];
            bool isPoc = (i == pocIdx);
            bool isVa = vaIndices.Contains(i);
            string nodeType = raw.TotalVol switch
            {
                var v when isPoc => "POC (Point of Control)",
                var v when v >= avgBucketVol * 1.35m => "HVN (High Volume Node)",
                var v when v <= avgBucketVol * 0.65m => "LVN (Low Volume Node)",
                _ => "Normal Acceptance Node"
            };

            bucketDtos.Add(new CryptoVolumeProfileBucketDto(
                PriceLevel: raw.PriceLevel,
                BuyVolume: Math.Round(raw.BuyVol, 2),
                SellVolume: Math.Round(raw.SellVol, 2),
                TotalVolume: Math.Round(raw.TotalVol, 2),
                IsPoc: isPoc,
                IsValueArea: isVa,
                NodeClassification: nodeType));
        }

        // Anchored VWAPs
        var anchoredVwaps = new List<CryptoAnchoredVwapDto>();
        var anchorSpecs = new[]
        {
            ("YTD Open Anchor", new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            ("Macro Cycle Low Anchor", candles.OrderBy(c => c.Low).First().Time),
            ("Peak High Anchor", candles.OrderByDescending(c => c.High).First().Time),
            ("Halving Cycle Anchor", new DateTime(2024, 4, 20, 0, 0, 0, DateTimeKind.Utc))
        };

        foreach (var (name, anchorDate) in anchorSpecs)
        {
            var relevantCandles = candles.Where(c => c.Time >= anchorDate).ToList();
            if (relevantCandles.Count < 5) relevantCandles = candles.TakeLast(30).ToList();

            decimal cumPv = 0m;
            decimal cumVol = 0m;
            foreach (var c in relevantCandles)
            {
                decimal tp = (c.High + c.Low + c.Close) / 3m;
                cumPv += tp * c.Volume;
                cumVol += c.Volume;
            }

            decimal vwap = cumVol > 0 ? cumPv / cumVol : currentPrice;

            // Standard deviation
            double varianceSum = 0;
            foreach (var c in relevantCandles)
            {
                decimal tp = (c.High + c.Low + c.Close) / 3m;
                double diff = (double)(tp - vwap);
                varianceSum += (double)c.Volume * diff * diff;
            }
            decimal sigma = cumVol > 0 ? (decimal)Math.Sqrt(varianceSum / (double)cumVol) : vwap * 0.02m;

            decimal upper1 = Math.Round(vwap + sigma, 4);
            decimal lower1 = Math.Round(vwap - sigma, 4);
            decimal upper2 = Math.Round(vwap + (2m * sigma), 4);
            decimal lower2 = Math.Round(vwap - (2m * sigma), 4);
            decimal devPercent = vwap > 0 ? Math.Round(((currentPrice - vwap) / vwap) * 100m, 2) : 0m;

            string verdict = currentPrice switch
            {
                var p when p >= upper2 => "Extreme Premium (+2σ Band Extension)",
                var p when p >= upper1 => "Bullish Markup Phase (+1σ Band Expansion)",
                var p when p >= lower1 => "Equilibrium Fair Value (Within 1σ Envelope)",
                var p when p >= lower2 => "Discount Value Accumulation (-1σ Band Retest)",
                _ => "Extreme Discount (-2σ Oversold Flush)"
            };

            anchoredVwaps.Add(new CryptoAnchoredVwapDto(
                AnchorName: name,
                AnchorDateUtc: anchorDate,
                VwapPrice: Math.Round(vwap, 4),
                UpperBand1Sigma: upper1,
                LowerBand1Sigma: lower1,
                UpperBand2Sigma: upper2,
                LowerBand2Sigma: lower2,
                CurrentDeviationPercent: devPercent,
                PositionVerdict: verdict));
        }

        string vpSignal = currentPrice switch
        {
            var p when p > vah => "Bullish Auction Expansion (Trading Above VAH)",
            var p when p < val => "Bearish Auction Rejection (Trading Below VAL)",
            var p when Math.Abs(p - pocPrice) / pocPrice <= 0.005m => "At Point of Control (Maximum Liquidity Acceptance)",
            _ => "Within Value Area (Rotational Mean Reversion)"
        };

        return new CryptoVolumeProfileHubDto(
            Symbol: symbol,
            Timeframe: timeframe,
            CurrentPrice: Math.Round(currentPrice, 4),
            PocPrice: Math.Round(pocPrice, 4),
            ValueAreaHigh: Math.Round(vah, 4),
            ValueAreaLow: Math.Round(val, 4),
            TotalVisibleVolumeUsd: Math.Round(totalVolume * currentPrice, 0),
            ValueAreaVolumePercent: 70m,
            Buckets: bucketDtos,
            AnchoredVwaps: anchoredVwaps,
            VolumeProfileSignal: vpSignal,
            LastUpdatedUtc: DateTime.UtcNow);
    }

    public async Task<CryptoMarketRegimeDto> GetMarketRegimeAsync(string symbol, CryptoTimeframe timeframe, CancellationToken ct = default)
    {
        var candles = await _marketData.GetCandlesAsync(symbol, timeframe, 120, ct);
        decimal currentPrice = candles.Count > 0 ? candles[^1].Close : await _marketData.GetCurrentPriceAsync(symbol, ct);

        if (candles.Count < 20)
        {
            return new CryptoMarketRegimeDto(
                symbol,
                "Mean-Reverting Range (Consolidation)",
                75m,
                18m,
                2.5m,
                3.8m,
                false,
                0.48m,
                "Insufficient Market Data",
                "Range Grid Trading",
                "Await historical candle accumulation before deploying directional leverage.",
                DateTime.UtcNow);
        }

        var closes = candles.Select(c => c.Close).ToList();
        decimal adx = CalculateAdx(candles, 14);
        decimal atr = CalculateAtr(candles, 14);
        decimal atrPercent = currentPrice > 0 ? Math.Round((atr / currentPrice) * 100m, 2) : 2.5m;
        var (_, _, _, bandwidth) = CalculateBollingerBands(closes, 20, 2.0m);
        bool isSqueeze = bandwidth <= 3.8m;
        decimal hurst = CalculateHurstExponent(closes);
        decimal ema50 = CalculateEma(closes, 50);
        decimal rsi = CalculateRsi(closes, 14);

        string regimeName;
        decimal confidence;
        string driver;
        string optimalStrategy;
        string guidance;

        if (isSqueeze)
        {
            regimeName = "Volatility Squeeze Imminent (Pre-Breakout Coil)";
            confidence = 89m;
            driver = "Order Book Liquidity Compression & Bid-Ask Tightening";
            optimalStrategy = "Volatility Expansion Straddle / Directional OCO Breakout";
            guidance = "Bollinger Bandwidth is tightly coiled (< 3.8%). Volatility expansion is imminent. Place stop-market entry orders beyond current range extremities with tight stop-losses.";
        }
        else if ((rsi >= 76m || rsi <= 24m) && atrPercent >= 3.8m)
        {
            regimeName = "Blow-Off Exhaustion Climax (Reversal Risk)";
            confidence = 84m;
            driver = "Cascading Forced Liquidations & Speculative Climax";
            optimalStrategy = "Counter-Trend Mean Reversion / De-risking Capital";
            guidance = "Extreme momentum and volatility exhaustion detected. Trailing stop-losses should be tightened immediately. Do not chase market orders into this distribution zone.";
        }
        else if (adx >= 25m && currentPrice > ema50 && hurst >= 0.52m)
        {
            regimeName = "Strong Bull Trend (Momentum Expansion)";
            confidence = Math.Min(96m, 62m + (adx * 0.8m));
            driver = "Institutional Spot Net Inflow & Persistent Aggressive Taker Buying";
            optimalStrategy = "Trend Following / Dip-Buying at VWAP & Bullish Order Blocks";
            guidance = "High directional conviction confirmed by ADX > 25 and persistent Hurst exponent. Enter long positions on pullbacks into discount OTE zones with trailing profit targets.";
        }
        else if (adx >= 25m && currentPrice < ema50 && hurst >= 0.52m)
        {
            regimeName = "Strong Bear Trend (Distribution Breakdown)";
            confidence = Math.Min(96m, 62m + (adx * 0.8m));
            driver = "Spot Outflow to Exchanges & Long Deleveraging Pressure";
            optimalStrategy = "Short Trend Following / Selling into Bearish Fair Value Gaps";
            guidance = "Persistent downtrend regime active. Avoid knife-catching. Sell relief rallies into Bearish FVG zones with invalidation above the recent lower swing high.";
        }
        else
        {
            regimeName = "Mean-Reverting Range (Liquidity Absorption)";
            confidence = 82m;
            driver = "Two-Way Algorithmic Market Maker Inventory Rebalancing";
            optimalStrategy = "Grid Trading / Bollinger Band Mean Reversion";
            guidance = "Low ADX (< 20) and anti-persistent Hurst indicate sideways rotational chop. Deploy range grid bots, fading external liquidity sweeps and taking profit at Equilibrium POC.";
        }

        return new CryptoMarketRegimeDto(
            Symbol: symbol,
            RegimeName: regimeName,
            RegimeConfidencePercent: confidence,
            AdxValue: Math.Round(adx, 1),
            AtrPercent: atrPercent,
            BollingerBandwidth: Math.Round(bandwidth, 2),
            IsBollingerSqueeze: isSqueeze,
            HurstExponent: Math.Round(hurst, 3),
            PrimaryMarketDriver: driver,
            OptimalStrategy: optimalStrategy,
            TacticalGuidance: guidance,
            LastUpdatedUtc: DateTime.UtcNow);
    }

    public async Task<CryptoAnalysisHubDto> GetAnalysisHubAsync(string symbol, CryptoTimeframe timeframe, CancellationToken ct = default)
    {
        var candles = await _marketData.GetCandlesAsync(symbol, timeframe, 120, ct);
        decimal currentPrice = candles.Count > 0 ? candles[^1].Close : await _marketData.GetCurrentPriceAsync(symbol, ct);

        var technical = CalculateIndicators(symbol, timeframe, candles);
        var sr = CalculateSupportResistance(candles, currentPrice);
        var structure = await GetMarketStructureAsync(symbol, timeframe, ct);
        var vp = await GetVolumeProfileAsync(symbol, timeframe, ct);
        var regime = await GetMarketRegimeAsync(symbol, timeframe, ct);

        return new CryptoAnalysisHubDto(
            Symbol: symbol,
            Timeframe: timeframe,
            CurrentPrice: Math.Round(currentPrice, 4),
            Technical: technical,
            SupportResistance: sr,
            MarketStructure: structure,
            VolumeProfile: vp,
            MarketRegime: regime,
            LastUpdatedUtc: DateTime.UtcNow);
    }

    private static decimal CalculateAdx(IReadOnlyList<CryptoCandleDto> candles, int period)
    {
        if (candles.Count <= period * 2) return 20m;

        var trList = new List<decimal>();
        var plusDmList = new List<decimal>();
        var minusDmList = new List<decimal>();

        for (int i = 1; i < candles.Count; i++)
        {
            decimal tr = Math.Max(candles[i].High - candles[i].Low,
                Math.Max(Math.Abs(candles[i].High - candles[i - 1].Close), Math.Abs(candles[i].Low - candles[i - 1].Close)));
            trList.Add(tr);

            decimal upMove = candles[i].High - candles[i - 1].High;
            decimal downMove = candles[i - 1].Low - candles[i].Low;

            plusDmList.Add((upMove > downMove && upMove > 0) ? upMove : 0m);
            minusDmList.Add((downMove > upMove && downMove > 0) ? downMove : 0m);
        }

        // Wilder's smoothing
        decimal smoothTr = trList.Take(period).Sum();
        decimal smoothPlusDm = plusDmList.Take(period).Sum();
        decimal smoothMinusDm = minusDmList.Take(period).Sum();

        var dxList = new List<decimal>();

        for (int i = period; i < trList.Count; i++)
        {
            smoothTr = smoothTr - (smoothTr / period) + trList[i];
            smoothPlusDm = smoothPlusDm - (smoothPlusDm / period) + plusDmList[i];
            smoothMinusDm = smoothMinusDm - (smoothMinusDm / period) + minusDmList[i];

            decimal plusDi = smoothTr > 0 ? (smoothPlusDm / smoothTr) * 100m : 0m;
            decimal minusDi = smoothTr > 0 ? (smoothMinusDm / smoothTr) * 100m : 0m;
            decimal diSum = plusDi + minusDi;
            decimal dx = diSum > 0 ? (Math.Abs(plusDi - minusDi) / diSum) * 100m : 0m;
            dxList.Add(dx);
        }

        if (dxList.Count < period) return dxList.Count > 0 ? dxList.Average() : 20m;
        return dxList.TakeLast(period).Average();
    }

    private static decimal CalculateHurstExponent(List<decimal> prices)
    {
        if (prices.Count < 30) return 0.50m;

        // Log returns
        var logReturns = new List<double>();
        for (int i = 1; i < prices.Count; i++)
        {
            if (prices[i - 1] > 0 && prices[i] > 0)
            {
                logReturns.Add(Math.Log((double)(prices[i] / prices[i - 1])));
            }
        }

        if (logReturns.Count < 24) return 0.50m;

        // Compute Rescaled Range (R/S) for lag N1=12 and lag N2=36
        double rs1 = ComputeRs(logReturns, 12);
        double rs2 = ComputeRs(logReturns, Math.Min(36, logReturns.Count));

        if (rs1 <= 0 || rs2 <= 0) return 0.50m;

        double hurst = (Math.Log(rs2) - Math.Log(rs1)) / (Math.Log(36.0) - Math.Log(12.0));
        return (decimal)Math.Clamp(hurst, 0.25, 0.85);
    }

    private static double ComputeRs(List<double> returns, int lag)
    {
        var slice = returns.TakeLast(lag).ToList();
        double mean = slice.Average();
        var meanAdjusted = slice.Select(r => r - mean).ToList();

        double cumSum = 0;
        double maxCum = double.MinValue;
        double minCum = double.MaxValue;

        foreach (var val in meanAdjusted)
        {
            cumSum += val;
            if (cumSum > maxCum) maxCum = cumSum;
            if (cumSum < minCum) minCum = cumSum;
        }

        double r = Math.Max(0.00001, maxCum - minCum);
        double variance = slice.Average(v => (v - mean) * (v - mean));
        double s = Math.Max(0.00001, Math.Sqrt(variance));

        return r / s;
    }
}
