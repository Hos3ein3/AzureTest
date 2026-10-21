using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Forex.Services;

public sealed class ForexMarketDataService : IForexMarketDataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ForexMarketDataService> _logger;

    private static readonly ConcurrentDictionary<string, decimal> _cachedBasePrices = new(StringComparer.OrdinalIgnoreCase);
    private static DateTime _lastFrankfurterFetchUtc = DateTime.MinValue;

    private static readonly Dictionary<string, (string DisplayName, decimal DefaultPrice, decimal BaseSpread, string Category)> PairDefinitions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Majors
        ["EUR/USD"] = ("EUR/USD (Euro / US Dollar)", 1.0850m, 1.2m, "Major"),
        ["GBP/USD"] = ("GBP/USD (British Pound / US Dollar)", 1.2980m, 1.5m, "Major"),
        ["USD/JPY"] = ("USD/JPY (US Dollar / Japanese Yen)", 152.40m, 1.4m, "Major"),
        ["USD/CHF"] = ("USD/CHF (US Dollar / Swiss Franc)", 0.8660m, 1.6m, "Major"),
        ["AUD/USD"] = ("AUD/USD (Australian Dollar / US Dollar)", 0.6650m, 1.3m, "Major"),
        ["USD/CAD"] = ("USD/CAD (US Dollar / Canadian Dollar)", 1.3820m, 1.6m, "Major"),
        ["NZD/USD"] = ("NZD/USD (New Zealand Dollar / US Dollar)", 0.6010m, 1.8m, "Major"),

        // Minors / Crosses
        ["EUR/GBP"] = ("EUR/GBP (Euro / British Pound)", 0.8360m, 1.8m, "Minor"),
        ["EUR/JPY"] = ("EUR/JPY (Euro / Japanese Yen)", 165.35m, 1.9m, "Minor"),
        ["GBP/JPY"] = ("GBP/JPY (Pound / Japanese Yen)", 197.80m, 2.2m, "Minor"),
        ["AUD/JPY"] = ("AUD/JPY (Aussie / Japanese Yen)", 101.35m, 2.0m, "Minor"),
        ["EUR/AUD"] = ("EUR/AUD (Euro / Aussie)", 1.6315m, 2.4m, "Minor"),

        // Metals & Indices
        ["XAU/USD"] = ("XAU/USD (Gold / US Dollar)", 2735.50m, 2.5m, "Metals"),
        ["XAG/USD"] = ("XAG/USD (Silver / US Dollar)", 34.20m, 2.0m, "Metals"),
        ["DXY"] = ("DXY (US Dollar Index)", 103.85m, 1.5m, "Indices")
    };

    public ForexMarketDataService(HttpClient httpClient, ILogger<ForexMarketDataService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Initialize default prices if not present
        foreach (var kvp in PairDefinitions)
        {
            _cachedBasePrices.TryAdd(kvp.Key, kvp.Value.DefaultPrice);
        }
    }

    private async Task RefreshFrankfurterRatesAsync(CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow - _lastFrankfurterFetchUtc < TimeSpan.FromMinutes(10))
            return;

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await _httpClient.GetAsync("https://api.frankfurter.dev/v1/latest?base=USD", cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cts.Token);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("rates", out var ratesElement))
                {
                    if (ratesElement.TryGetProperty("EUR", out var eurRate) && eurRate.GetDecimal() > 0)
                    {
                        var eurPerUsd = eurRate.GetDecimal();
                        _cachedBasePrices["EUR/USD"] = Math.Round(1m / eurPerUsd, 5);
                    }
                    if (ratesElement.TryGetProperty("GBP", out var gbpRate) && gbpRate.GetDecimal() > 0)
                    {
                        var gbpPerUsd = gbpRate.GetDecimal();
                        _cachedBasePrices["GBP/USD"] = Math.Round(1m / gbpPerUsd, 5);
                    }
                    if (ratesElement.TryGetProperty("JPY", out var jpyRate) && jpyRate.GetDecimal() > 0)
                    {
                        _cachedBasePrices["USD/JPY"] = Math.Round(jpyRate.GetDecimal(), 3);
                    }
                    if (ratesElement.TryGetProperty("CHF", out var chfRate) && chfRate.GetDecimal() > 0)
                    {
                        _cachedBasePrices["USD/CHF"] = Math.Round(chfRate.GetDecimal(), 5);
                    }
                    if (ratesElement.TryGetProperty("CAD", out var cadRate) && cadRate.GetDecimal() > 0)
                    {
                        _cachedBasePrices["USD/CAD"] = Math.Round(cadRate.GetDecimal(), 5);
                    }
                    if (ratesElement.TryGetProperty("AUD", out var audRate) && audRate.GetDecimal() > 0)
                    {
                        _cachedBasePrices["AUD/USD"] = Math.Round(1m / audRate.GetDecimal(), 5);
                    }
                    if (ratesElement.TryGetProperty("NZD", out var nzdRate) && nzdRate.GetDecimal() > 0)
                    {
                        _cachedBasePrices["NZD/USD"] = Math.Round(1m / nzdRate.GetDecimal(), 5);
                    }

                    // Derived Crosses
                    if (_cachedBasePrices.TryGetValue("EUR/USD", out var eurUsd) && _cachedBasePrices.TryGetValue("GBP/USD", out var gbpUsd) && gbpUsd > 0)
                    {
                        _cachedBasePrices["EUR/GBP"] = Math.Round(eurUsd / gbpUsd, 5);
                    }
                    if (_cachedBasePrices.TryGetValue("EUR/USD", out var eurUsd2) && _cachedBasePrices.TryGetValue("USD/JPY", out var usdJpy))
                    {
                        _cachedBasePrices["EUR/JPY"] = Math.Round(eurUsd2 * usdJpy, 3);
                    }
                    if (_cachedBasePrices.TryGetValue("GBP/USD", out var gbpUsd2) && _cachedBasePrices.TryGetValue("USD/JPY", out var usdJpy2))
                    {
                        _cachedBasePrices["GBP/JPY"] = Math.Round(gbpUsd2 * usdJpy2, 3);
                    }

                    _lastFrankfurterFetchUtc = DateTime.UtcNow;
                    _logger.LogInformation("Successfully refreshed live rates from Frankfurter Central Bank feed.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Frankfurter rate fetch skipped or timed out. Falling back to cached price engine: {Message}", ex.Message);
        }
    }

    public async Task<MarketOverviewDto> GetMarketOverviewAsync(ForexMarketFilter filter, CancellationToken cancellationToken = default)
    {
        await RefreshFrankfurterRatesAsync(cancellationToken);

        var quotes = new List<ForexQuoteDto>();
        var metalsAndIndices = new List<ForexQuoteDto>();

        foreach (var def in PairDefinitions)
        {
            var symbol = def.Key;
            var isMetalOrIndex = def.Value.Category is "Metals" or "Indices";

            if (!string.IsNullOrWhiteSpace(filter.Category) && !string.Equals(filter.Category, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(def.Value.Category, filter.Category, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                if (!symbol.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) &&
                    !def.Value.DisplayName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            var quote = await GetQuoteAsync(symbol, cancellationToken);
            if (isMetalOrIndex)
            {
                metalsAndIndices.Add(quote);
            }
            else
            {
                quotes.Add(quote);
            }
        }

        var sessions = GetMarketSessions();
        return new MarketOverviewDto(quotes, sessions, metalsAndIndices);
    }

    public async Task<ForexQuoteDto> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default)
    {
        await RefreshFrankfurterRatesAsync(cancellationToken);

        var normalized = symbol.ToUpperInvariant().Trim();
        if (!PairDefinitions.TryGetValue(normalized, out var def))
        {
            def = (normalized, 1.0000m, 1.5m, "Minor");
        }

        var basePrice = _cachedBasePrices.TryGetValue(normalized, out var p) ? p : def.DefaultPrice;

        // Apply slight realistic market jitter based on time
        var jitterHash = (DateTime.UtcNow.Minute * 60 + DateTime.UtcNow.Second + normalized.GetHashCode()) % 100;
        var jitterFactor = (jitterHash - 50) * 0.00003m;
        var currentMid = basePrice * (1m + jitterFactor);

        var isJpy = normalized.Contains("JPY", StringComparison.OrdinalIgnoreCase);
        var isGold = normalized.Contains("XAU", StringComparison.OrdinalIgnoreCase);
        var pipMultiplier = isJpy ? 0.01m : (isGold ? 0.1m : 0.0001m);

        var spreadPips = def.BaseSpread;
        var halfSpread = (spreadPips * pipMultiplier) / 2m;

        var bid = Math.Round(currentMid - halfSpread, isJpy ? 3 : (isGold ? 2 : 5));
        var ask = Math.Round(currentMid + halfSpread, isJpy ? 3 : (isGold ? 2 : 5));

        var dailyChangePercent = Math.Round(((jitterHash - 45) * 0.025m), 2);
        var high24h = Math.Round(bid * 1.006m, isJpy ? 3 : 5);
        var low24h = Math.Round(bid * 0.994m, isJpy ? 3 : 5);

        return new ForexQuoteDto(
            normalized,
            def.DisplayName,
            bid,
            ask,
            spreadPips,
            dailyChangePercent,
            high24h,
            low24h,
            DateTime.UtcNow,
            def.Category
        );
    }

    public async Task<ForexTickDto> GetLiveTickAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var quote = await GetQuoteAsync(symbol, cancellationToken);
        return new ForexTickDto(quote.Symbol, quote.Bid, quote.Ask, DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<ForexCandleDto>> GetCandlesAsync(string symbol, ForexTimeframe timeframe, int count = 100, CancellationToken cancellationToken = default)
    {
        var quote = await GetQuoteAsync(symbol, cancellationToken);
        var currentPrice = quote.Bid;

        var candles = new List<ForexCandleDto>();
        var minutes = (int)timeframe;
        var candleDuration = TimeSpan.FromMinutes(minutes);

        var now = DateTime.UtcNow;
        var isJpy = symbol.Contains("JPY", StringComparison.OrdinalIgnoreCase);
        var isGold = symbol.Contains("XAU", StringComparison.OrdinalIgnoreCase);
        var scale = isJpy ? 3 : (isGold ? 2 : 5);

        // Deterministic synthetic historical walk backwards
        var seed = Math.Abs(symbol.GetHashCode() + (int)timeframe);
        var rng = new Random(seed);

        decimal walkPrice = currentPrice;

        for (int i = 0; i < count; i++)
        {
            var candleTime = now - TimeSpan.FromMinutes(minutes * (count - i));

            var volatility = currentPrice * 0.0015m * (decimal)Math.Sqrt(minutes / 15.0);
            var change = (decimal)(rng.NextDouble() * 2.0 - 1.0) * volatility;

            var open = walkPrice;
            var close = open + change;

            var highOffset = (decimal)(rng.NextDouble()) * volatility * 0.8m;
            var lowOffset = (decimal)(rng.NextDouble()) * volatility * 0.8m;

            var high = Math.Max(open, close) + highOffset;
            var low = Math.Min(open, close) - lowOffset;
            var volume = (decimal)rng.Next(150, 4500);

            candles.Add(new ForexCandleDto(
                candleTime,
                Math.Round(open, scale),
                Math.Round(high, scale),
                Math.Round(low, scale),
                Math.Round(close, scale),
                volume
            ));

            walkPrice = close;
        }

        return candles;
    }

    public IReadOnlyList<ForexSessionDto> GetMarketSessions()
    {
        var now = DateTime.UtcNow;
        var currentTime = now.TimeOfDay;

        // Session definitions in UTC
        // Sydney: 21:00 UTC - 06:00 UTC (crosses midnight)
        // Tokyo: 00:00 UTC - 09:00 UTC
        // London: 07:00 UTC - 16:00 UTC
        // New York: 12:00 UTC - 21:00 UTC

        var sydneyOpen = new TimeSpan(21, 0, 0);
        var sydneyClose = new TimeSpan(6, 0, 0);
        var isSydneyOpen = currentTime >= sydneyOpen || currentTime < sydneyClose;

        var tokyoOpen = new TimeSpan(0, 0, 0);
        var tokyoClose = new TimeSpan(9, 0, 0);
        var isTokyoOpen = currentTime >= tokyoOpen && currentTime < tokyoClose;

        var londonOpen = new TimeSpan(7, 0, 0);
        var londonClose = new TimeSpan(16, 0, 0);
        var isLondonOpen = currentTime >= londonOpen && currentTime < londonClose;

        var nyOpen = new TimeSpan(12, 0, 0);
        var nyClose = new TimeSpan(21, 0, 0);
        var isNyOpen = currentTime >= nyOpen && currentTime < nyClose;

        var overlapText = (isLondonOpen && isNyOpen)
            ? "London / New York Overlap (Highest Liquidity & Volume)"
            : (isTokyoOpen && isSydneyOpen)
                ? "Asian / Sydney Overlap"
                : (isLondonOpen && isTokyoOpen)
                    ? "Tokyo / London Morning Overlap"
                    : "Single Session Active";

        var list = new List<ForexSessionDto>
        {
            new(
                ForexMarketSession.Sydney,
                "Sydney",
                "AEDT / AEST",
                sydneyOpen,
                sydneyClose,
                isSydneyOpen,
                CalculateTimeToNext(currentTime, sydneyOpen, sydneyClose, isSydneyOpen),
                isSydneyOpen && isTokyoOpen ? "Active Overlap: Tokyo" : "Normal"
            ),
            new(
                ForexMarketSession.Tokyo,
                "Tokyo",
                "JST (UTC+9)",
                tokyoOpen,
                tokyoClose,
                isTokyoOpen,
                CalculateTimeToNext(currentTime, tokyoOpen, tokyoClose, isTokyoOpen),
                isTokyoOpen && isLondonOpen ? "Active Overlap: London" : "Normal"
            ),
            new(
                ForexMarketSession.London,
                "London",
                "GMT / BST",
                londonOpen,
                londonClose,
                isLondonOpen,
                CalculateTimeToNext(currentTime, londonOpen, londonClose, isLondonOpen),
                isLondonOpen && isNyOpen ? overlapText : "Normal"
            ),
            new(
                ForexMarketSession.NewYork,
                "New York",
                "EST / EDT",
                nyOpen,
                nyClose,
                isNyOpen,
                CalculateTimeToNext(currentTime, nyOpen, nyClose, isNyOpen),
                isLondonOpen && isNyOpen ? overlapText : "Normal"
            )
        };

        return list;
    }

    private static TimeSpan CalculateTimeToNext(TimeSpan current, TimeSpan open, TimeSpan close, bool isOpen)
    {
        if (isOpen)
        {
            // Time until close
            if (close > current) return close - current;
            return (TimeSpan.FromHours(24) - current) + close;
        }
        else
        {
            // Time until open
            if (open > current) return open - current;
            return (TimeSpan.FromHours(24) - current) + open;
        }
    }
}
