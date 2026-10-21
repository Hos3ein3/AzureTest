using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;

namespace AzureTest.Application.Crypto.Services;

public class CryptoStrategyService : ICryptoStrategyService
{
    private readonly ICryptoMarketDataService _marketData;
    private readonly ICryptoAnalysisService _analysis;

    public CryptoStrategyService(ICryptoMarketDataService marketData, ICryptoAnalysisService analysis)
    {
        _marketData = marketData;
        _analysis = analysis;
    }

    public async Task<BacktestResultDto> RunBacktestAsync(BacktestRequestDto request, CancellationToken ct = default)
    {
        // 1. Fetch historical candles for backtesting
        int candleCount = request.Timeframe switch
        {
            CryptoTimeframe.M1 => Math.Min(500, request.DaysBack * 24 * 6),
            CryptoTimeframe.M5 => Math.Min(500, request.DaysBack * 24 * 2),
            CryptoTimeframe.M15 => Math.Min(500, request.DaysBack * 24),
            CryptoTimeframe.H1 => Math.Min(500, request.DaysBack * 24),
            CryptoTimeframe.H4 => Math.Min(500, request.DaysBack * 6),
            CryptoTimeframe.D1 => Math.Min(500, request.DaysBack),
            _ => 150
        };

        var candles = await _marketData.GetCandlesAsync(request.Symbol, request.Timeframe, Math.Max(60, candleCount), ct);
        if (candles.Count < 30)
        {
            return CreateEmptyBacktest(request);
        }

        decimal equity = request.InitialCapital;
        decimal peakEquity = equity;
        decimal maxDrawdown = 0m;
        decimal totalFees = 0m;

        var trades = new List<BacktestTradeDto>();
        var equityCurve = new List<EquityCurvePointDto>
        {
            new(candles[0].Time, equity, 0m)
        };

        bool inPosition = false;
        decimal entryPrice = 0m;
        DateTime entryTime = DateTime.MinValue;
        decimal quantity = 0m;
        string currentSide = "LONG";
        decimal positionCost = 0m;

        decimal feeRate = request.CommissionPercent / 100m;
        decimal slipRate = request.SlippagePercent / 100m;

        // 2. Step through candles sequentially simulating real-time signals
        for (int i = 25; i < candles.Count; i++)
        {
            var historicalSlice = candles.Take(i + 1).ToList();
            var currentCandle = candles[i];
            var indicators = _analysis.CalculateIndicators(request.Symbol, request.Timeframe, historicalSlice);

            // Signal generation based on strategy
            bool buySignal = false;
            bool sellSignal = false;

            switch (request.StrategyType)
            {
                case CryptoStrategyType.TrendFollowing:
                    // Golden Cross: Fast EMA crosses above Slow EMA + RSI healthy (> 50 and < 70)
                    buySignal = indicators.Ema9 > indicators.Ema21 && indicators.Rsi14 is > 50 and < 70 && indicators.MacdHistogram > 0;
                    sellSignal = indicators.Ema9 < indicators.Ema21 || indicators.Rsi14 > 78 || indicators.MacdHistogram < 0;
                    break;

                case CryptoStrategyType.MeanReversion:
                    // Bounce from lower Bollinger Band with RSI oversold (< 32)
                    buySignal = currentCandle.Close <= indicators.BollingerLower * 1.005m && indicators.Rsi14 < 35;
                    sellSignal = currentCandle.Close >= indicators.BollingerUpper * 0.995m || indicators.Rsi14 > 65;
                    break;

                case CryptoStrategyType.Breakout:
                    // Breakout: Squeeze (bandwidth < 5%) followed by expansion above upper band with volume surge
                    bool isSqueeze = indicators.BollingerBandwidth < 6.0m;
                    buySignal = isSqueeze && currentCandle.Close > indicators.BollingerUpper && currentCandle.Volume > indicators.VolumeMa * 1.3m;
                    sellSignal = currentCandle.Close < indicators.BollingerMiddle || indicators.Rsi14 > 80;
                    break;

                default:
                    buySignal = indicators.OverallSignal is "Strong Buy" or "Buy";
                    sellSignal = indicators.OverallSignal is "Strong Sell" or "Sell";
                    break;
            }

            // Execute trading logic
            if (!inPosition && buySignal)
            {
                // Enter Long position
                decimal executedPrice = currentCandle.Close * (1.0m + slipRate);
                decimal tradeSizeUsd = equity * 0.95m; // 95% equity allocated
                quantity = executedPrice > 0 ? tradeSizeUsd / executedPrice : 0m;
                decimal entryFee = tradeSizeUsd * feeRate;

                equity -= entryFee;
                totalFees += entryFee;
                entryPrice = executedPrice;
                entryTime = currentCandle.Time;
                positionCost = tradeSizeUsd;
                inPosition = true;
                currentSide = "LONG";
            }
            else if (inPosition && sellSignal)
            {
                // Exit Long position
                decimal exitPrice = currentCandle.Close * (1.0m - slipRate);
                decimal grossProceeds = quantity * exitPrice;
                decimal exitFee = grossProceeds * feeRate;
                decimal netPnl = grossProceeds - positionCost - exitFee;
                decimal pnlPct = positionCost > 0 ? (netPnl / positionCost) * 100m : 0m;

                equity += (grossProceeds - positionCost - exitFee);
                totalFees += exitFee;

                trades.Add(new BacktestTradeDto(
                    EntryTime: entryTime,
                    ExitTime: currentCandle.Time,
                    Side: currentSide,
                    EntryPrice: Math.Round(entryPrice, 4),
                    ExitPrice: Math.Round(exitPrice, 4),
                    Quantity: Math.Round(quantity, 4),
                    Pnl: Math.Round(netPnl, 2),
                    PnlPercent: Math.Round(pnlPct, 2),
                    ExitReason: "Strategy Exit Signal",
                    FeePaid: Math.Round(exitFee, 2)));

                inPosition = false;
            }

            // Track Drawdown & Equity Curve
            if (equity > peakEquity) peakEquity = equity;
            decimal currentDrawdown = peakEquity > 0 ? ((peakEquity - equity) / peakEquity) * 100m : 0m;
            if (currentDrawdown > maxDrawdown) maxDrawdown = currentDrawdown;

            equityCurve.Add(new EquityCurvePointDto(currentCandle.Time, Math.Round(equity, 2), Math.Round(currentDrawdown, 2)));
        }

        // Close open trade at final candle price if still in position
        if (inPosition)
        {
            var finalCandle = candles[^1];
            decimal exitPrice = finalCandle.Close * (1.0m - slipRate);
            decimal grossProceeds = quantity * exitPrice;
            decimal exitFee = grossProceeds * feeRate;
            decimal netPnl = grossProceeds - positionCost - exitFee;
            decimal pnlPct = positionCost > 0 ? (netPnl / positionCost) * 100m : 0m;

            equity += (grossProceeds - positionCost - exitFee);
            totalFees += exitFee;

            trades.Add(new BacktestTradeDto(
                EntryTime: entryTime,
                ExitTime: finalCandle.Time,
                Side: currentSide,
                EntryPrice: Math.Round(entryPrice, 4),
                ExitPrice: Math.Round(exitPrice, 4),
                Quantity: Math.Round(quantity, 4),
                Pnl: Math.Round(netPnl, 2),
                PnlPercent: Math.Round(pnlPct, 2),
                ExitReason: "Backtest Period Ended",
                FeePaid: Math.Round(exitFee, 2)));
        }

        // 3. Performance Metrics
        int totalTrades = trades.Count;
        int winTrades = trades.Count(t => t.Pnl > 0);
        int lossTrades = trades.Count(t => t.Pnl <= 0);
        decimal winRate = totalTrades > 0 ? ((decimal)winTrades / totalTrades) * 100m : 0m;

        decimal grossWins = trades.Where(t => t.Pnl > 0).Sum(t => t.Pnl);
        decimal grossLosses = Math.Abs(trades.Where(t => t.Pnl < 0).Sum(t => t.Pnl));
        decimal profitFactor = grossLosses > 0 ? grossWins / grossLosses : (grossWins > 0 ? 9.99m : 1.0m);

        decimal netProfit = equity - request.InitialCapital;
        decimal returnPct = request.InitialCapital > 0 ? (netProfit / request.InitialCapital) * 100m : 0m;

        // Sharpe Ratio: (Mean trade return - 0) / StdDev of returns
        decimal sharpe = CalculateSharpeRatio(trades);

        // 4. Walk-Forward Cross Validation
        var walkForward = PerformWalkForwardValidation(trades, request.InitialCapital);

        return new BacktestResultDto(
            Id: Guid.NewGuid(),
            StrategyName: $"{request.StrategyType} ({request.Symbol})",
            StrategyType: request.StrategyType,
            Symbol: request.Symbol,
            Timeframe: request.Timeframe,
            InitialCapital: request.InitialCapital,
            FinalCapital: Math.Round(equity, 2),
            NetProfit: Math.Round(netProfit, 2),
            ReturnPercent: Math.Round(returnPct, 2),
            TotalTrades: totalTrades,
            WinningTrades: winTrades,
            LosingTrades: lossTrades,
            WinRate: Math.Round(winRate, 2),
            ProfitFactor: Math.Round(profitFactor, 2),
            MaxDrawdownPercent: Math.Round(maxDrawdown, 2),
            SharpeRatio: Math.Round(sharpe, 2),
            TotalFeesPaid: Math.Round(totalFees, 2),
            Trades: trades,
            EquityCurve: equityCurve,
            WalkForward: walkForward);
    }

    public WalkForwardResultDto PerformWalkForwardValidation(IReadOnlyList<BacktestTradeDto> trades, decimal initialCapital)
    {
        if (trades == null || trades.Count < 4)
        {
            return new WalkForwardResultDto(
                0m, 0m, 0m, "Insufficient Data", "Caution", "At least 4 completed trades are required for walk-forward cross validation.",
                0m, 0m, 0m, "High", "At least 4 completed trades are required for walk-forward cross validation.");
        }

        // Split trades: 60% In-Sample (training), 40% Out-of-Sample (validation)
        int splitIndex = (int)(trades.Count * 0.60);
        var inSampleTrades = trades.Take(splitIndex).ToList();
        var outOfSampleTrades = trades.Skip(splitIndex).ToList();

        decimal inSamplePnl = inSampleTrades.Sum(t => t.Pnl);
        decimal inSampleReturn = initialCapital > 0 ? (inSamplePnl / initialCapital) * 100m : 0m;

        decimal outOfSamplePnl = outOfSampleTrades.Sum(t => t.Pnl);
        decimal outOfSampleReturn = initialCapital > 0 ? (outOfSamplePnl / initialCapital) * 100m : 0m;

        // Overfitting Score: Ratio of out-of-sample performance drop
        decimal dropPct = inSampleReturn > 0
            ? Math.Max(0m, ((inSampleReturn - outOfSampleReturn) / inSampleReturn) * 100m)
            : (outOfSampleReturn < 0 ? 50m : 0m);

        string stability;
        string rating;
        string notes;

        if (outOfSampleReturn >= inSampleReturn * 0.70m && outOfSampleReturn > 0)
        {
            stability = "High";
            rating = "Pass";
            notes = "Strategy performs consistently in out-of-sample data. Low probability of curve-fitting.";
        }
        else if (outOfSampleReturn > 0)
        {
            stability = "Moderate";
            rating = "Caution";
            notes = "Moderate performance decay in out-of-sample period. Verify parameters with wider ATR/volatility bands.";
        }
        else
        {
            stability = "Unstable";
            rating = "Fail";
            notes = "Severe curve-fitting detected. In-sample gains vanished in out-of-sample testing. Do not trade live.";
        }

        decimal inSampleSharpe = CalculateSharpeRatio(inSampleTrades);
        decimal outOfSampleSharpe = CalculateSharpeRatio(outOfSampleTrades);
        string risk = rating == "Pass" ? "Low" : (rating == "Caution" ? "Moderate" : "High");

        return new WalkForwardResultDto(
            InSampleReturnPercent: Math.Round(inSampleReturn, 2),
            OutOfSampleReturnPercent: Math.Round(outOfSampleReturn, 2),
            OverfittingScore: Math.Round(dropPct, 2),
            ParameterStability: stability,
            RobustnessRating: rating,
            Notes: notes,
            InSampleSharpe: Math.Round(inSampleSharpe, 2),
            OutOfSampleSharpe: Math.Round(outOfSampleSharpe, 2),
            DegradationPercent: Math.Round(dropPct, 1),
            OverfittingRisk: risk,
            Recommendation: notes);
    }

    private static decimal CalculateSharpeRatio(IReadOnlyList<BacktestTradeDto> trades)
    {
        if (trades.Count < 2) return 1.0m;

        var returns = trades.Select(t => t.PnlPercent).ToList();
        decimal mean = returns.Average();
        decimal sumSquares = returns.Sum(r => (r - mean) * (r - mean));
        decimal stdDev = (decimal)Math.Sqrt((double)(sumSquares / returns.Count));

        if (stdDev == 0m) return 1.0m;
        // Annualize assuming ~250 trading periods
        return (mean / stdDev) * (decimal)Math.Sqrt(250);
    }

    private static BacktestResultDto CreateEmptyBacktest(BacktestRequestDto req) => new(
        Id: Guid.NewGuid(),
        StrategyName: req.StrategyType.ToString(),
        StrategyType: req.StrategyType,
        Symbol: req.Symbol,
        Timeframe: req.Timeframe,
        InitialCapital: req.InitialCapital,
        FinalCapital: req.InitialCapital,
        NetProfit: 0m,
        ReturnPercent: 0m,
        TotalTrades: 0,
        WinningTrades: 0,
        LosingTrades: 0,
        WinRate: 0m,
        ProfitFactor: 0m,
        MaxDrawdownPercent: 0m,
        SharpeRatio: 0m,
        TotalFeesPaid: 0m,
        Trades: Array.Empty<BacktestTradeDto>(),
        EquityCurve: Array.Empty<EquityCurvePointDto>(),
        WalkForward: new WalkForwardResultDto(0m, 0m, 0m, "None", "Caution", "Insufficient historical candles to execute backtest."));
}
