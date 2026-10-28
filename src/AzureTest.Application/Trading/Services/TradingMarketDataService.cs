using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Forex.Services;
using AzureTest.Contracts.DTOs.Trading;
using AzureTest.Domain.Entities.Trading.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Trading.Services;

public interface ITradingMarketDataService
{
    Task<IReadOnlyList<TradingQuoteDto>> GetAllQuotesAsync(HashSet<string>? watchlistedSymbols = null, CancellationToken ct = default);
    Task<TradingQuoteDto?> GetQuoteAsync(string symbol, HashSet<string>? watchlistedSymbols = null, CancellationToken ct = default);
    Task<decimal> GetCurrentPriceAsync(string symbol, decimal fallback = 100m, CancellationToken ct = default);
    Task RefreshRealWorldDataAsync(CancellationToken ct = default);

    IReadOnlyList<TradingQuoteDto> GetAllQuotes(HashSet<string>? watchlistedSymbols = null);
    TradingQuoteDto? GetQuote(string symbol, HashSet<string>? watchlistedSymbols = null);
    decimal GetCurrentPrice(string symbol, decimal fallback = 100m);
}

public sealed class TradingMarketDataService : ITradingMarketDataService
{
    private readonly ICryptoMarketDataService _cryptoMarketData;
    private readonly IForexMarketDataService _forexMarketData;
    private readonly ILogger<TradingMarketDataService> _logger;

    private sealed record LiveQuoteState(
        decimal Price,
        decimal Change,
        decimal ChangePercent,
        decimal High,
        decimal Low,
        decimal Volume,
        bool IsSimulated
    );

    private static readonly ConcurrentDictionary<string, LiveQuoteState> LiveCache = new(StringComparer.OrdinalIgnoreCase);
    private static DateTime _lastRefreshUtc = DateTime.MinValue;
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromSeconds(3);

    private static readonly List<(string Symbol, string Name, AssetClass AssetClass, decimal BasePrice, decimal ChangePercent, decimal High, decimal Low, decimal Volume)> Catalog = new()
    {
        // Cryptocurrencies (Real-World Live Feed via Binance Public API)
        ("BTCUSDT", "Bitcoin", AssetClass.Crypto, 67450.00m, 2.45m, 68100.00m, 65890.00m, 2450000000m),
        ("ETHUSDT", "Ethereum", AssetClass.Crypto, 3520.50m, 1.80m, 3560.00m, 3440.00m, 1200000000m),
        ("SOLUSDT", "Solana", AssetClass.Crypto, 182.40m, 5.60m, 185.90m, 171.50m, 850000000m),
        ("BNBUSDT", "BNB Chain", AssetClass.Crypto, 594.80m, -0.40m, 602.00m, 588.00m, 420000000m),
        ("XRPUSDT", "Ripple XRP", AssetClass.Crypto, 0.5840m, 3.20m, 0.5980m, 0.5620m, 380000000m),
        ("DOGEUSDT", "Dogecoin", AssetClass.Crypto, 0.1420m, 6.80m, 0.1490m, 0.1310m, 290000000m),
        ("AVAXUSDT", "Avalanche", AssetClass.Crypto, 29.50m, -1.20m, 30.80m, 28.70m, 140000000m),

        // Forex Currencies (Real-World Live Feed via Frankfurter European Central Bank API)
        ("EUR/USD", "Euro / US Dollar", AssetClass.Forex, 1.0845m, -0.15m, 1.0872m, 1.0820m, 4500000000m),
        ("GBP/USD", "British Pound / US Dollar", AssetClass.Forex, 1.2980m, 0.22m, 1.3015m, 1.2940m, 3200000000m),
        ("USD/JPY", "US Dollar / Japanese Yen", AssetClass.Forex, 152.35m, 0.45m, 152.80m, 151.70m, 3800000000m),
        ("AUD/USD", "Australian Dollar / US Dollar", AssetClass.Forex, 0.6582m, -0.30m, 0.6610m, 0.6550m, 1800000000m),
        ("USD/CAD", "US Dollar / Canadian Dollar", AssetClass.Forex, 1.3855m, 0.12m, 1.3890m, 1.3820m, 1900000000m),
        ("USD/CHF", "US Dollar / Swiss Franc", AssetClass.Forex, 0.8665m, -0.08m, 0.8690m, 0.8640m, 1400000000m),

        // Stocks (Simulated Paper Feed)
        ("AAPL", "Apple Inc.", AssetClass.Stock, 232.50m, 1.15m, 234.20m, 229.80m, 58000000m),
        ("MSFT", "Microsoft Corporation", AssetClass.Stock, 428.10m, 0.85m, 431.00m, 424.50m, 24000000m),
        ("NVDA", "NVIDIA Corporation", AssetClass.Stock, 141.20m, 3.40m, 143.50m, 137.00m, 72000000m),
        ("TSLA", "Tesla Inc.", AssetClass.Stock, 218.40m, -1.90m, 224.00m, 215.20m, 48000000m),
        ("AMZN", "Amazon.com Inc.", AssetClass.Stock, 187.90m, 1.30m, 189.50m, 185.00m, 36000000m),
        ("GOOGL", "Alphabet Inc.", AssetClass.Stock, 166.30m, 0.60m, 168.00m, 164.80m, 22000000m),
        ("META", "Meta Platforms Inc.", AssetClass.Stock, 578.40m, 2.10m, 584.00m, 571.20m, 18000000m),

        // Indices (Simulated Paper Feed)
        ("SPX", "S&P 500 Index", AssetClass.Index, 5815.20m, 0.42m, 5832.00m, 5798.00m, 3800000000m),
        ("NDX", "NASDAQ 100", AssetClass.Index, 20340.50m, 0.78m, 20420.00m, 20210.00m, 4900000000m),
        ("DJI", "Dow Jones Industrial", AssetClass.Index, 42230.00m, -0.18m, 42410.00m, 42120.00m, 2100000000m),
        ("DAX", "Germany DAX 40", AssetClass.Index, 19480.00m, 0.35m, 19550.00m, 19390.00m, 1200000000m),
        ("FTSE", "UK FTSE 100", AssetClass.Index, 8240.00m, -0.10m, 8280.00m, 8210.00m, 980000000m),

        // Futures & Commodities (Simulated Paper Feed)
        ("XAUUSD", "Gold Futures (Troy Oz)", AssetClass.Future, 2745.80m, 0.95m, 2758.00m, 2728.00m, 1500000000m),
        ("XAGUSD", "Silver Futures", AssetClass.Future, 33.70m, 1.45m, 34.20m, 33.10m, 480000000m),
        ("USOIL", "Crude Oil WTI", AssetClass.Future, 71.20m, -1.65m, 72.80m, 70.40m, 890000000m),
        ("NATGAS", "Natural Gas Futures", AssetClass.Future, 2.450m, 2.10m, 2.520m, 2.380m, 210000000m),
        ("ES", "E-Mini S&P 500 Futures", AssetClass.Future, 5825.00m, 0.40m, 5840.00m, 5805.00m, 2700000000m)
    };

    static TradingMarketDataService()
    {
        // Pre-populate cache with initial catalog baseline data
        foreach (var item in Catalog)
        {
            bool isSim = item.AssetClass == AssetClass.Stock || item.AssetClass == AssetClass.Future || item.AssetClass == AssetClass.Index;
            LiveCache[item.Symbol] = new LiveQuoteState(
                item.BasePrice,
                item.BasePrice * (item.ChangePercent / 100m),
                item.ChangePercent,
                item.High,
                item.Low,
                item.Volume,
                isSim
            );
        }
    }

    public TradingMarketDataService(
        ICryptoMarketDataService cryptoMarketData,
        IForexMarketDataService forexMarketData,
        ILogger<TradingMarketDataService> logger)
    {
        _cryptoMarketData = cryptoMarketData;
        _forexMarketData = forexMarketData;
        _logger = logger;
    }

    public async Task RefreshRealWorldDataAsync(CancellationToken ct = default)
    {
        if (DateTime.UtcNow - _lastRefreshUtc < CacheExpiry && !LiveCache.IsEmpty)
        {
            return;
        }

        if (!await RefreshLock.WaitAsync(TimeSpan.FromSeconds(2), ct))
        {
            return;
        }

        try
        {
            if (DateTime.UtcNow - _lastRefreshUtc < CacheExpiry && !LiveCache.IsEmpty)
            {
                return;
            }

            // 1. Fetch Real-World Crypto prices via Binance Public API
            var cryptoTasks = Catalog.Where(c => c.AssetClass == AssetClass.Crypto).Select(async c =>
            {
                try
                {
                    var livePrice = await _cryptoMarketData.GetCurrentPriceAsync(c.Symbol, ct);
                    if (livePrice > 0)
                    {
                        var delta = livePrice - c.BasePrice;
                        var pct = c.BasePrice > 0 ? Math.Round((delta / c.BasePrice) * 100m, 2) : 0m;
                        var high = Math.Max(c.High, livePrice);
                        var low = Math.Min(c.Low, livePrice);

                        LiveCache[c.Symbol] = new LiveQuoteState(
                            livePrice,
                            delta,
                            pct,
                            high,
                            low,
                            c.Volume,
                            IsSimulated: false // Real-world Binance feed
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "TradingMarketDataService: Failed to update live Binance crypto price for {Symbol}.", c.Symbol);
                }
            });

            // 2. Fetch Real-World Forex rates via Frankfurter (European Central Bank)
            var forexTasks = Catalog.Where(c => c.AssetClass == AssetClass.Forex).Select(async f =>
            {
                try
                {
                    var quote = await _forexMarketData.GetQuoteAsync(f.Symbol, ct);
                    if (quote != null && quote.Bid > 0)
                    {
                        var change = quote.Bid * (quote.ChangePercent / 100m);
                        LiveCache[f.Symbol] = new LiveQuoteState(
                            quote.Bid,
                            change,
                            quote.ChangePercent,
                            quote.High24h,
                            quote.Low24h,
                            f.Volume,
                            IsSimulated: false // Real-world Frankfurter ECB feed
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "TradingMarketDataService: Failed to update live Frankfurter forex quote for {Symbol}.", f.Symbol);
                }
            });

            // 3. For Stocks, Indices, Futures, ensure simulated flags are retained
            foreach (var s in Catalog.Where(c => c.AssetClass == AssetClass.Stock || c.AssetClass == AssetClass.Index || c.AssetClass == AssetClass.Future))
            {
                if (!LiveCache.ContainsKey(s.Symbol))
                {
                    LiveCache[s.Symbol] = new LiveQuoteState(
                        s.BasePrice,
                        s.BasePrice * (s.ChangePercent / 100m),
                        s.ChangePercent,
                        s.High,
                        s.Low,
                        s.Volume,
                        IsSimulated: true // Simulated paper feed
                    );
                }
            }

            await Task.WhenAll(cryptoTasks.Concat(forexTasks));
            _lastRefreshUtc = DateTime.UtcNow;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    public async Task<IReadOnlyList<TradingQuoteDto>> GetAllQuotesAsync(HashSet<string>? watchlistedSymbols = null, CancellationToken ct = default)
    {
        await RefreshRealWorldDataAsync(ct);
        return GetAllQuotes(watchlistedSymbols);
    }

    public async Task<TradingQuoteDto?> GetQuoteAsync(string symbol, HashSet<string>? watchlistedSymbols = null, CancellationToken ct = default)
    {
        if (DateTime.UtcNow - _lastRefreshUtc > CacheExpiry)
        {
            await RefreshRealWorldDataAsync(ct);
        }
        return GetQuote(symbol, watchlistedSymbols);
    }

    public async Task<decimal> GetCurrentPriceAsync(string symbol, decimal fallback = 100m, CancellationToken ct = default)
    {
        if (DateTime.UtcNow - _lastRefreshUtc > CacheExpiry)
        {
            await RefreshRealWorldDataAsync(ct);
        }
        return GetCurrentPrice(symbol, fallback);
    }

    public IReadOnlyList<TradingQuoteDto> GetAllQuotes(HashSet<string>? watchlistedSymbols = null)
    {
        watchlistedSymbols ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return Catalog.Select(c =>
        {
            if (LiveCache.TryGetValue(c.Symbol, out var live))
            {
                return new TradingQuoteDto(
                    c.Symbol,
                    c.Name,
                    c.AssetClass,
                    live.Price,
                    live.Change,
                    live.ChangePercent,
                    live.High,
                    live.Low,
                    live.Volume,
                    watchlistedSymbols.Contains(c.Symbol),
                    live.IsSimulated
                );
            }

            bool isSim = c.AssetClass == AssetClass.Stock || c.AssetClass == AssetClass.Future || c.AssetClass == AssetClass.Index;
            return new TradingQuoteDto(
                c.Symbol,
                c.Name,
                c.AssetClass,
                c.BasePrice,
                c.BasePrice * (c.ChangePercent / 100m),
                c.ChangePercent,
                c.High,
                c.Low,
                c.Volume,
                watchlistedSymbols.Contains(c.Symbol),
                isSim
            );
        }).ToList();
    }

    public TradingQuoteDto? GetQuote(string symbol, HashSet<string>? watchlistedSymbols = null)
    {
        var item = Catalog.FirstOrDefault(c => c.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        if (item == default) return null;

        watchlistedSymbols ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (LiveCache.TryGetValue(item.Symbol, out var live))
        {
            return new TradingQuoteDto(
                item.Symbol,
                item.Name,
                item.AssetClass,
                live.Price,
                live.Change,
                live.ChangePercent,
                live.High,
                live.Low,
                live.Volume,
                watchlistedSymbols.Contains(item.Symbol),
                live.IsSimulated
            );
        }

        bool isSim = item.AssetClass == AssetClass.Stock || item.AssetClass == AssetClass.Future || item.AssetClass == AssetClass.Index;
        return new TradingQuoteDto(
            item.Symbol,
            item.Name,
            item.AssetClass,
            item.BasePrice,
            item.BasePrice * (item.ChangePercent / 100m),
            item.ChangePercent,
            item.High,
            item.Low,
            item.Volume,
            watchlistedSymbols.Contains(item.Symbol),
            isSim
        );
    }

    public decimal GetCurrentPrice(string symbol, decimal fallback = 100m)
    {
        if (LiveCache.TryGetValue(symbol, out var live) && live.Price > 0)
        {
            return live.Price;
        }

        var item = Catalog.FirstOrDefault(c => c.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        return item != default ? item.BasePrice : fallback;
    }
}
