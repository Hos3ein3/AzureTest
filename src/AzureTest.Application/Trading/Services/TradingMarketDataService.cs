using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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
    Task RefreshSymbolsDataAsync(IEnumerable<string> symbols, CancellationToken ct = default);
    Task<IReadOnlyList<TradingQuoteDto>> SearchAssetsAsync(string query, HashSet<string>? watchlistedSymbols = null, CancellationToken ct = default);

    Task<TradingLeverageBracketDto> GetLeverageBracketAsync(string symbol, CancellationToken ct = default);
    Task<int> GetMaxLeverageAsync(string symbol, CancellationToken ct = default);
    TradingLeverageBracketDto GetLeverageBracket(string symbol);
    int GetMaxLeverage(string symbol);

    IReadOnlyList<TradingQuoteDto> GetAllQuotes(HashSet<string>? watchlistedSymbols = null);
    TradingQuoteDto? GetQuote(string symbol, HashSet<string>? watchlistedSymbols = null);
    decimal GetCurrentPrice(string symbol, decimal fallback = 100m);
}

public sealed record TradingCatalogItem(
    string Symbol,
    string Name,
    AssetClass AssetClass,
    decimal BasePrice,
    decimal ChangePercent,
    decimal High,
    decimal Low,
    decimal Volume,
    bool IsSimulated = false,
    string FeedSource = "binance",
    int MaxLeverage = 250
);

public sealed class TradingMarketDataService : ITradingMarketDataService
{
    private readonly ICryptoMarketDataService _cryptoMarketData;
    private readonly IForexMarketDataService _forexMarketData;
    private readonly ILogger<TradingMarketDataService> _logger;
    private readonly HttpClient _http;
    private readonly Microsoft.Extensions.Configuration.IConfiguration? _configuration;

    private static readonly ConcurrentDictionary<string, (TradingLeverageBracketDto Data, DateTime ExpiresAtUtc)> LeverageBracketCache = new(StringComparer.OrdinalIgnoreCase);

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
    private static readonly ConcurrentDictionary<string, TradingCatalogItem> DynamicCatalog = new(StringComparer.OrdinalIgnoreCase);
    private static DateTime _lastRefreshUtc = DateTime.MinValue;
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromSeconds(3);

    private static readonly List<TradingCatalogItem> Catalog = new()
    {
        // Cryptocurrencies (Real-World Live Feed via Binance Public API)
        new("BTCUSDT", "Bitcoin", AssetClass.Crypto, 67450.00m, 2.45m, 68100.00m, 65890.00m, 2450000000m, false, "binance", 125),
        new("ETHUSDT", "Ethereum", AssetClass.Crypto, 3520.50m, 1.80m, 3560.00m, 3440.00m, 1200000000m, false, "binance", 100),
        new("SOLUSDT", "Solana", AssetClass.Crypto, 102.50m, 2.80m, 106.50m, 99.20m, 850000000m, false, "binance", 75),
        new("BNBUSDT", "BNB Chain", AssetClass.Crypto, 594.80m, -0.40m, 602.00m, 588.00m, 420000000m, false, "binance", 75),
        new("XRPUSDT", "Ripple XRP", AssetClass.Crypto, 0.5840m, 3.20m, 0.5980m, 0.5620m, 380000000m, false, "binance", 75),
        new("DOGEUSDT", "Dogecoin", AssetClass.Crypto, 0.1420m, 6.80m, 0.1490m, 0.1310m, 290000000m, false, "binance", 75),
        new("AVAXUSDT", "Avalanche", AssetClass.Crypto, 29.50m, -1.20m, 30.80m, 28.70m, 140000000m, false, "binance", 50),
        new("OPUSDT", "Optimism", AssetClass.Crypto, 0.1112m, 0.63m, 0.1139m, 0.1096m, 26000000m, false, "binance", 50),
        new("ADAUSDT", "Cardano", AssetClass.Crypto, 0.3540m, 1.10m, 0.3620m, 0.3450m, 180000000m, false, "binance", 75),
        new("LINKUSDT", "Chainlink", AssetClass.Crypto, 11.65m, 2.10m, 11.90m, 11.20m, 95000000m, false, "binance", 75),
        new("SUIUSDT", "Sui Network", AssetClass.Crypto, 2.12m, 4.50m, 2.18m, 1.98m, 180000000m, false, "binance", 50),
        new("NEARUSDT", "NEAR Protocol", AssetClass.Crypto, 4.85m, 1.80m, 5.05m, 4.60m, 110000000m, false, "binance", 50),
        new("PEPEUSDT", "Pepe", AssetClass.Crypto, 0.0000104m, 3.80m, 0.000011m, 0.0000098m, 320000000m, false, "binance", 50),
        new("ARBUSDT", "Arbitrum", AssetClass.Crypto, 0.5280m, 0.90m, 0.5420m, 0.5110m, 78000000m, false, "binance", 50),
        new("APTUSDT", "Aptos", AssetClass.Crypto, 9.40m, 2.40m, 9.75m, 9.10m, 65000000m, false, "binance", 50),
        new("RENDERUSDT", "Render", AssetClass.Crypto, 5.65m, 3.20m, 5.85m, 5.35m, 72000000m, false, "binance", 50),
        new("FETUSDT", "Artificial Superintelligence", AssetClass.Crypto, 1.34m, 1.60m, 1.41m, 1.28m, 54000000m, false, "binance", 50),

        // Forex Currencies (Real-World Live Feed via Frankfurter European Central Bank API)
        new("EUR/USD", "Euro / US Dollar", AssetClass.Forex, 1.0845m, -0.15m, 1.0872m, 1.0820m, 4500000000m, false, "forex"),
        new("GBP/USD", "British Pound / US Dollar", AssetClass.Forex, 1.2980m, 0.22m, 1.3015m, 1.2940m, 3200000000m, false, "forex"),
        new("USD/JPY", "US Dollar / Japanese Yen", AssetClass.Forex, 152.35m, 0.45m, 152.80m, 151.70m, 3800000000m, false, "forex"),
        new("AUD/USD", "Australian Dollar / US Dollar", AssetClass.Forex, 0.6582m, -0.30m, 0.6610m, 0.6550m, 1800000000m, false, "forex"),
        new("USD/CAD", "US Dollar / Canadian Dollar", AssetClass.Forex, 1.3855m, 0.12m, 1.3890m, 1.3820m, 1900000000m, false, "forex"),
        new("USD/CHF", "US Dollar / Swiss Franc", AssetClass.Forex, 0.8665m, -0.08m, 0.8690m, 0.8640m, 1400000000m, false, "forex"),
        new("GBP/JPY", "British Pound / Japanese Yen", AssetClass.Forex, 197.80m, 0.35m, 198.40m, 196.90m, 1600000000m, false, "forex"),
        new("EUR/GBP", "Euro / British Pound", AssetClass.Forex, 0.8355m, -0.12m, 0.8380m, 0.8330m, 1500000000m, false, "forex"),

        // Stocks (Simulated Paper Feed)
        new("AAPL", "Apple Inc.", AssetClass.Stock, 232.50m, 1.15m, 234.20m, 229.80m, 58000000m, true, "simulated"),
        new("MSFT", "Microsoft Corporation", AssetClass.Stock, 428.10m, 0.85m, 431.00m, 424.50m, 24000000m, true, "simulated"),
        new("NVDA", "NVIDIA Corporation", AssetClass.Stock, 141.20m, 3.40m, 143.50m, 137.00m, 72000000m, true, "simulated"),
        new("TSLA", "Tesla Inc.", AssetClass.Stock, 218.40m, -1.90m, 224.00m, 215.20m, 48000000m, true, "simulated"),
        new("PLTR", "Palantir Technologies", AssetClass.Stock, 43.80m, 4.10m, 44.90m, 41.50m, 62000000m, true, "simulated"),
        new("AMD", "Advanced Micro Devices", AssetClass.Stock, 156.40m, 1.80m, 158.90m, 153.20m, 38000000m, true, "simulated"),
        new("COIN", "Coinbase Global Inc.", AssetClass.Stock, 215.60m, 3.80m, 221.00m, 209.40m, 19000000m, true, "simulated"),
        new("MSTR", "MicroStrategy Inc.", AssetClass.Stock, 248.50m, 6.20m, 255.00m, 236.00m, 28000000m, true, "simulated"),
        new("AMZN", "Amazon.com Inc.", AssetClass.Stock, 187.90m, 1.30m, 189.50m, 185.00m, 36000000m, true, "simulated"),
        new("GOOGL", "Alphabet Inc.", AssetClass.Stock, 166.30m, 0.60m, 168.00m, 164.80m, 22000000m, true, "simulated"),
        new("META", "Meta Platforms Inc.", AssetClass.Stock, 578.40m, 2.10m, 584.00m, 571.20m, 18000000m, true, "simulated"),

        // Indices (Simulated Paper Feed)
        new("SPX", "S&P 500 Index", AssetClass.Index, 5815.20m, 0.42m, 5832.00m, 5798.00m, 3800000000m, true, "simulated"),
        new("NDX", "NASDAQ 100", AssetClass.Index, 20340.50m, 0.78m, 20420.00m, 20210.00m, 4900000000m, true, "simulated"),
        new("DJI", "Dow Jones Industrial", AssetClass.Index, 42230.00m, -0.18m, 42410.00m, 42120.00m, 2100000000m, true, "simulated"),
        new("DAX", "Germany DAX 40", AssetClass.Index, 19480.00m, 0.35m, 19550.00m, 19390.00m, 1200000000m, true, "simulated"),
        new("FTSE", "UK FTSE 100", AssetClass.Index, 8240.00m, -0.10m, 8280.00m, 8210.00m, 980000000m, true, "simulated"),

        // Futures & Commodities (Simulated Paper Feed)
        new("XAUUSD", "Gold Futures (Troy Oz)", AssetClass.Future, 2745.80m, 0.95m, 2758.00m, 2728.00m, 1500000000m, true, "simulated"),
        new("XAGUSD", "Silver Futures", AssetClass.Future, 33.70m, 1.45m, 34.20m, 33.10m, 48000000m, true, "simulated"),
        new("USOIL", "Crude Oil WTI", AssetClass.Future, 71.20m, -1.65m, 72.80m, 70.40m, 890000000m, true, "simulated"),
        new("NATGAS", "Natural Gas Futures", AssetClass.Future, 2.450m, 2.10m, 2.520m, 2.380m, 21000000m, true, "simulated"),
        new("ES", "E-Mini S&P 500 Futures", AssetClass.Future, 5825.00m, 0.40m, 5840.00m, 5805.00m, 2700000000m, true, "simulated")
    };

    static TradingMarketDataService()
    {
        // Pre-populate cache with initial catalog baseline data
        foreach (var item in Catalog)
        {
            LiveCache[item.Symbol] = new LiveQuoteState(
                item.BasePrice,
                item.BasePrice * (item.ChangePercent / 100m),
                item.ChangePercent,
                item.High,
                item.Low,
                item.Volume,
                item.IsSimulated
            );
        }
    }

    public TradingMarketDataService(
        ICryptoMarketDataService cryptoMarketData,
        IForexMarketDataService forexMarketData,
        ILogger<TradingMarketDataService> logger,
        HttpClient? http = null,
        Microsoft.Extensions.Configuration.IConfiguration? configuration = null)
    {
        _cryptoMarketData = cryptoMarketData;
        _forexMarketData = forexMarketData;
        _logger = logger;
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(6) };
        _configuration = configuration;
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _http.DefaultRequestHeaders.Add("User-Agent", "AzureTest-TradingTerminal/1.0");
        }
    }

    private static async Task<HttpResponseMessage?> SendWithRetryAsync(
        HttpClient http,
        Func<HttpRequestMessage> requestFactory,
        ILogger logger,
        int maxRetries = 3,
        CancellationToken ct = default)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var request = requestFactory();
                var response = await http.SendAsync(request, ct);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                if ((int)response.StatusCode == 429 || (int)response.StatusCode >= 500)
                {
                    var delayMs = (int)(Math.Pow(2, attempt) * 150) + Random.Shared.Next(50, 150);
                    logger.LogWarning("HTTP call to {Uri} failed with status {StatusCode}. Attempt {Attempt}/{MaxRetries}, retrying in {DelayMs}ms.",
                        response.RequestMessage?.RequestUri, response.StatusCode, attempt, maxRetries, delayMs);
                    response.Dispose();

                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delayMs, ct);
                        continue;
                    }
                }

                return response;
            }
            catch (Exception ex) when (attempt < maxRetries && !ct.IsCancellationRequested)
            {
                var delayMs = (int)(Math.Pow(2, attempt) * 150) + Random.Shared.Next(50, 150);
                logger.LogWarning(ex, "HTTP request failed on attempt {Attempt}/{MaxRetries}. Retrying in {DelayMs}ms.", attempt, maxRetries, delayMs);
                await Task.Delay(delayMs, ct);
            }
        }

        return null;
    }

    private static void UpdateLiveCacheFromBinanceElement(JsonElement el)
    {
        if (el.TryGetProperty("symbol", out var sProp) &&
            el.TryGetProperty("lastPrice", out var lpProp) &&
            decimal.TryParse(lpProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price) &&
            price > 0)
        {
            var sym = sProp.GetString() ?? string.Empty;
            decimal change = decimal.TryParse(el.GetProperty("priceChange").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var cVal) ? cVal : 0m;
            decimal pct = decimal.TryParse(el.GetProperty("priceChangePercent").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var pVal) ? pVal : 0m;
            decimal high = decimal.TryParse(el.GetProperty("highPrice").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var hVal) ? hVal : price * 1.05m;
            decimal low = decimal.TryParse(el.GetProperty("lowPrice").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var lVal) ? lVal : price * 0.95m;
            decimal vol = decimal.TryParse(el.GetProperty("volume").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var vVal) ? vVal : 100000m;

            LiveCache[sym] = new LiveQuoteState(price, change, pct, high, low, vol, IsSimulated: false);
        }
    }

    private static void UpdateLiveCacheFromMexcElement(JsonElement root, string sym)
    {
        if (root.TryGetProperty("lastPrice", out var lpProp) &&
            decimal.TryParse(lpProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price) &&
            price > 0)
        {
            decimal change = root.TryGetProperty("priceChange", out var cProp) && decimal.TryParse(cProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var cVal) ? cVal : 0m;
            decimal pct = root.TryGetProperty("priceChangePercent", out var pProp) && decimal.TryParse(pProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var pVal) ? pVal * 100m : 0m;
            decimal high = root.TryGetProperty("highPrice", out var hProp) && decimal.TryParse(hProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var hVal) ? hVal : price * 1.05m;
            decimal low = root.TryGetProperty("lowPrice", out var lProp) && decimal.TryParse(lProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var lVal) ? lVal : price * 0.95m;
            decimal vol = root.TryGetProperty("volume", out var vProp) && decimal.TryParse(vProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var vVal) ? vVal : 100000m;

            var assetBase = sym.EndsWith("USDT") ? sym.Substring(0, sym.Length - 4) : sym;
            DynamicCatalog[sym] = new TradingCatalogItem(sym, $"{assetBase} / Tether USD", AssetClass.Crypto, price, pct, high, low, vol, IsSimulated: false, FeedSource: "mexc");
            LiveCache[sym] = new LiveQuoteState(price, change, pct, high, low, vol, IsSimulated: false);
        }
    }

    private async Task RefreshMexcSymbolAsync(string sym, CancellationToken ct = default)
    {
        try
        {
            var res = await _http.GetAsync($"https://api.mexc.com/api/v3/ticker/24hr?symbol={sym}", ct);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                UpdateLiveCacheFromMexcElement(doc.RootElement, sym);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed refreshing MEXC ticker for {Symbol}", sym);
        }
    }

    public async Task RefreshSymbolsDataAsync(IEnumerable<string> symbols, CancellationToken ct = default)
    {
        var symbolList = (symbols ?? Enumerable.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (symbolList.Count == 0)
        {
            return;
        }

        var cryptoSymbols = new List<string>();
        var forexSymbols = new List<string>();

        foreach (var sym in symbolList)
        {
            if (Catalog.Any(c => c.Symbol.Equals(sym, StringComparison.OrdinalIgnoreCase) && c.AssetClass == AssetClass.Forex) || sym.Contains('/'))
            {
                forexSymbols.Add(sym);
            }
            else if (!Catalog.Any(c => c.Symbol.Equals(sym, StringComparison.OrdinalIgnoreCase) && c.IsSimulated))
            {
                cryptoSymbols.Add(sym);
            }
        }

        var tasks = new List<Task>();

        if (cryptoSymbols.Count > 0)
        {
            var mexcSymbols = new List<string>();
            var binanceSymbols = new List<string>();

            foreach (var sym in cryptoSymbols)
            {
                if ((DynamicCatalog.TryGetValue(sym, out var catItem) && catItem.FeedSource == "mexc") ||
                    sym.Equals("BLESSUSDT", StringComparison.OrdinalIgnoreCase) ||
                    sym.Equals("AGTUSDT", StringComparison.OrdinalIgnoreCase))
                {
                    mexcSymbols.Add(sym);
                }
                else
                {
                    binanceSymbols.Add(sym);
                }
            }

            if (mexcSymbols.Count > 0)
            {
                tasks.AddRange(mexcSymbols.Select(m => RefreshMexcSymbolAsync(m, ct)));
            }

            if (binanceSymbols.Count > 0)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        string url;
                        if (binanceSymbols.Count == 1)
                        {
                            url = $"https://api.binance.com/api/v3/ticker/24hr?symbol={binanceSymbols[0]}";
                        }
                        else
                        {
                            var symbolsJson = JsonSerializer.Serialize(binanceSymbols);
                            url = $"https://api.binance.com/api/v3/ticker/24hr?symbols={Uri.EscapeDataString(symbolsJson)}";
                        }

                        var res = await SendWithRetryAsync(
                            _http,
                            () => new HttpRequestMessage(HttpMethod.Get, url),
                            _logger,
                            maxRetries: 2,
                            ct: ct);

                        if (res != null && res.IsSuccessStatusCode)
                        {
                            var json = await res.Content.ReadAsStringAsync(ct);
                            res.Dispose();
                            using var doc = JsonDocument.Parse(json);

                            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var el in doc.RootElement.EnumerateArray())
                                {
                                    UpdateLiveCacheFromBinanceElement(el);
                                }
                            }
                            else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                            {
                                UpdateLiveCacheFromBinanceElement(doc.RootElement);
                            }
                        }
                        else
                        {
                            res?.Dispose();
                            // Fallback to MEXC for individual symbols if Binance batch or single ticker failed
                            foreach (var s in binanceSymbols)
                            {
                                await RefreshMexcSymbolAsync(s, ct);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "TradingMarketDataService: Resilient fetch failed for {Count} active crypto symbols. Attempting MEXC fallback.", binanceSymbols.Count);
                        foreach (var s in binanceSymbols)
                        {
                            await RefreshMexcSymbolAsync(s, ct);
                        }
                    }
                }, ct));
            }
        }

        if (forexSymbols.Count > 0)
        {
            tasks.AddRange(forexSymbols.Select(async f =>
            {
                try
                {
                    var quote = await _forexMarketData.GetQuoteAsync(f, ct);
                    if (quote != null && quote.Bid > 0)
                    {
                        var change = quote.Bid * (quote.ChangePercent / 100m);
                        LiveCache[f] = new LiveQuoteState(
                            quote.Bid,
                            change,
                            quote.ChangePercent,
                            quote.High24h,
                            quote.Low24h,
                            1000000m,
                            IsSimulated: false
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to refresh forex rate for {Symbol}", f);
                }
            }));
        }

        await Task.WhenAll(tasks);
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

            var binanceCryptoItems = Catalog.Where(c => c.AssetClass == AssetClass.Crypto)
                .Concat(DynamicCatalog.Values.Where(d => d.AssetClass == AssetClass.Crypto && !d.IsSimulated && (d.FeedSource == "binance" || string.IsNullOrEmpty(d.FeedSource))));

            var mexcCryptoItems = DynamicCatalog.Values.Where(d => d.AssetClass == AssetClass.Crypto && !d.IsSimulated && d.FeedSource == "mexc")
                .Select(d => d.Symbol)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var mexcTasks = mexcCryptoItems.Select(m => RefreshMexcSymbolAsync(m, ct));

            // 1. Fetch Real-World Crypto prices via Binance Public API in a single high-efficiency batch call with retry resilience
            var cryptoBatchTask = Task.Run(async () =>
            {
                var cryptoList = binanceCryptoItems.ToList();
                try
                {
                    var res = await SendWithRetryAsync(
                        _http,
                        () => new HttpRequestMessage(HttpMethod.Get, "https://api.binance.com/api/v3/ticker/24hr"),
                        _logger,
                        maxRetries: 3,
                        ct: ct);

                    if (res != null && res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync(ct);
                        res.Dispose();
                        using var doc = JsonDocument.Parse(json);
                        var tickerMap = new Dictionary<string, (decimal price, decimal change, decimal pct, decimal high, decimal low, decimal vol)>(StringComparer.OrdinalIgnoreCase);

                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            if (el.TryGetProperty("symbol", out var sProp) &&
                                el.TryGetProperty("lastPrice", out var lpProp) &&
                                decimal.TryParse(lpProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price) &&
                                price > 0)
                            {
                                var sym = sProp.GetString() ?? string.Empty;
                                decimal change = decimal.TryParse(el.GetProperty("priceChange").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var cVal) ? cVal : 0m;
                                decimal pct = decimal.TryParse(el.GetProperty("priceChangePercent").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var pVal) ? pVal : 0m;
                                decimal high = decimal.TryParse(el.GetProperty("highPrice").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var hVal) ? hVal : price * 1.05m;
                                decimal low = decimal.TryParse(el.GetProperty("lowPrice").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var lVal) ? lVal : price * 0.95m;
                                decimal vol = decimal.TryParse(el.GetProperty("volume").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var vVal) ? vVal : 100000m;

                                tickerMap[sym] = (price, change, pct, high, low, vol);
                            }
                        }

                        foreach (var c in cryptoList)
                        {
                            if (tickerMap.TryGetValue(c.Symbol, out var t))
                            {
                                LiveCache[c.Symbol] = new LiveQuoteState(
                                    t.price,
                                    t.change,
                                    t.pct,
                                    t.high,
                                    t.low,
                                    t.vol,
                                    IsSimulated: false
                                );
                            }
                        }
                    }
                    else
                    {
                        res?.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "TradingMarketDataService: Batch update from Binance failed. Keeping existing live cache.");
                }
            }, ct);

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
                            IsSimulated: false
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "TradingMarketDataService: Failed to update live Frankfurter forex quote for {Symbol}.", f.Symbol);
                }
            });

            // 3. For Simulated assets (Stocks, Indices, Futures), apply continuous realistic micro-ticks (random walk +-0.03%)
            var random = Random.Shared;
            foreach (var s in Catalog.Where(c => c.IsSimulated).Concat(DynamicCatalog.Values.Where(d => d.IsSimulated)))
            {
                if (LiveCache.TryGetValue(s.Symbol, out var current))
                {
                    var jitter = (decimal)((random.NextDouble() * 0.0006) - 0.0003);
                    var newPrice = Math.Round(current.Price * (1m + jitter), s.PricePrecision());
                    var newChange = newPrice - s.BasePrice;
                    var newPct = s.BasePrice > 0 ? Math.Round((newChange / s.BasePrice) * 100m, 2) : 0m;
                    var high = Math.Max(current.High, newPrice);
                    var low = Math.Min(current.Low, newPrice);

                    LiveCache[s.Symbol] = new LiveQuoteState(
                        newPrice,
                        newChange,
                        newPct,
                        high,
                        low,
                        s.Volume,
                        IsSimulated: true
                    );
                }
            }

            await Task.WhenAll(new[] { cryptoBatchTask }.Concat(forexTasks).Concat(mexcTasks));
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
        watchlistedSymbols ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cleanSym = (symbol ?? string.Empty).Trim().ToUpperInvariant();

        if (DynamicCatalog.TryGetValue(cleanSym, out var dynQuote) && dynQuote.FeedSource == "mexc")
        {
            await RefreshMexcSymbolAsync(cleanSym, ct);
        }
        else if (DateTime.UtcNow - _lastRefreshUtc > CacheExpiry)
        {
            await RefreshRealWorldDataAsync(ct);
        }

        var quote = GetQuote(cleanSym, watchlistedSymbols);
        if (quote != null) return quote;

        // Dynamic lookup via Binance, MEXC, and DexScreener if symbol is not yet in Catalog
        var results = await SearchAssetsAsync(cleanSym, watchlistedSymbols, ct);
        var match = results.FirstOrDefault(r => r.Symbol.Equals(cleanSym, StringComparison.OrdinalIgnoreCase))
                    ?? results.FirstOrDefault();

        if (match != null) return match;

        // Resilient fallback for any requested asset name
        var price = 10.00m;
        var item = new TradingCatalogItem(cleanSym, $"{cleanSym} Paper Asset", AssetClass.Crypto, price, 0m, price * 1.02m, price * 0.98m, 100000m, IsSimulated: true, FeedSource: "simulated", MaxLeverage: GetKnownMaxLeverage(cleanSym));
        DynamicCatalog[cleanSym] = item;
        LiveCache[cleanSym] = new LiveQuoteState(price, 0m, 0m, price * 1.02m, price * 0.98m, 100000m, IsSimulated: true);

        return new TradingQuoteDto(cleanSym, item.Name, AssetClass.Crypto, price, 0m, 0m, price * 1.02m, price * 0.98m, 100000m, watchlistedSymbols.Contains(cleanSym), IsSimulated: true, FeedSource: "simulated", MaxLeverage: item.MaxLeverage);
    }

    public async Task<decimal> GetCurrentPriceAsync(string symbol, decimal fallback = 100m, CancellationToken ct = default)
    {
        var cleanSym = (symbol ?? string.Empty).Trim().ToUpperInvariant();
        if (LiveCache.TryGetValue(cleanSym, out var live) && live.Price > 0)
        {
            return live.Price;
        }

        // Direct resilient Binance ticker fetch if crypto symbol
        if (!cleanSym.Contains('/') && !Catalog.Any(c => c.Symbol.Equals(cleanSym, StringComparison.OrdinalIgnoreCase) && c.IsSimulated))
        {
            var isMexc = (DynamicCatalog.TryGetValue(cleanSym, out var dc) && dc.FeedSource == "mexc") ||
                         cleanSym.Equals("BLESSUSDT", StringComparison.OrdinalIgnoreCase) ||
                         cleanSym.Equals("AGTUSDT", StringComparison.OrdinalIgnoreCase);

            if (!isMexc)
            {
                try
                {
                    var res = await SendWithRetryAsync(
                        _http,
                        () => new HttpRequestMessage(HttpMethod.Get, $"https://api.binance.com/api/v3/ticker/price?symbol={cleanSym}"),
                        _logger,
                        maxRetries: 2,
                        ct: ct);

                    if (res != null && res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync(ct);
                        res.Dispose();
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("price", out var pProp) &&
                            decimal.TryParse(pProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var p) && p > 0)
                        {
                            if (LiveCache.TryGetValue(cleanSym, out var existing))
                            {
                                LiveCache[cleanSym] = existing with { Price = p };
                            }
                            else
                            {
                                LiveCache[cleanSym] = new LiveQuoteState(p, 0m, 0m, p * 1.05m, p * 0.95m, 100000m, IsSimulated: false);
                            }
                            return p;
                        }
                    }
                    else
                    {
                        res?.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GetCurrentPriceAsync: Resilient fetch failed for {Symbol}.", cleanSym);
                }
            }

            // Fallback / Direct MEXC price check
            try
            {
                var mexcRes = await _http.GetAsync($"https://api.mexc.com/api/v3/ticker/price?symbol={cleanSym}", ct);
                if (mexcRes.IsSuccessStatusCode)
                {
                    var json = await mexcRes.Content.ReadAsStringAsync(ct);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("price", out var pProp) &&
                        decimal.TryParse(pProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var p) && p > 0)
                    {
                        if (LiveCache.TryGetValue(cleanSym, out var existing))
                        {
                            LiveCache[cleanSym] = existing with { Price = p };
                        }
                        else
                        {
                            LiveCache[cleanSym] = new LiveQuoteState(p, 0m, 0m, p * 1.05m, p * 0.95m, 100000m, IsSimulated: false);
                        }
                        return p;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "GetCurrentPriceAsync: MEXC check failed for {Symbol}.", cleanSym);
            }
        }

        var quote = await GetQuoteAsync(cleanSym, null, ct);
        if (quote != null && quote.Price > 0)
        {
            return quote.Price;
        }

        if (DynamicCatalog.TryGetValue(cleanSym, out var dyn) && dyn.BasePrice > 0)
        {
            return dyn.BasePrice;
        }

        return fallback;
    }

    public async Task<IReadOnlyList<TradingQuoteDto>> SearchAssetsAsync(string query, HashSet<string>? watchlistedSymbols = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<TradingQuoteDto>();

        watchlistedSymbols ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cleanQ = query.Trim().ToUpperInvariant();
        var results = new Dictionary<string, TradingQuoteDto>(StringComparer.OrdinalIgnoreCase);

        // 1. Search in Catalog
        foreach (var c in Catalog.Where(c => c.Symbol.Contains(cleanQ, StringComparison.OrdinalIgnoreCase) || c.Name.Contains(cleanQ, StringComparison.OrdinalIgnoreCase)))
        {
            var quote = GetQuote(c.Symbol, watchlistedSymbols);
            if (quote != null) results[quote.Symbol] = quote;
        }

        // 2. Search in DynamicCatalog (only trust verified non-dexscreener items upfront)
        foreach (var d in DynamicCatalog.Values.Where(d => d.FeedSource != "dexscreener" && (d.Symbol.Contains(cleanQ, StringComparison.OrdinalIgnoreCase) || d.Name.Contains(cleanQ, StringComparison.OrdinalIgnoreCase))))
        {
            var quote = GetQuote(d.Symbol, watchlistedSymbols);
            if (quote != null) results[quote.Symbol] = quote;
        }

        var targetSym = cleanQ.EndsWith("USDT") ? cleanQ : cleanQ + "USDT";

        // 3. Live search via Binance Public 24hr Ticker API (Tier-1 Primary crypto exchange)
        if (cleanQ.Length >= 2 && (!results.ContainsKey(targetSym) || (DynamicCatalog.TryGetValue(targetSym, out var dynBin) && dynBin.FeedSource == "binance")))
        {
            try
            {
                var binanceSym = targetSym;
                var res = await _http.GetAsync($"https://api.binance.com/api/v3/ticker/24hr?symbol={binanceSym}", ct);
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync(ct);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("lastPrice", out var lpProp) &&
                        decimal.TryParse(lpProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price) &&
                        price > 0)
                    {
                        decimal change = decimal.TryParse(root.GetProperty("priceChange").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var cVal) ? cVal : 0m;
                        decimal pct = decimal.TryParse(root.GetProperty("priceChangePercent").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var pVal) ? pVal : 0m;
                        decimal high = decimal.TryParse(root.GetProperty("highPrice").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var hVal) ? hVal : price * 1.05m;
                        decimal low = decimal.TryParse(root.GetProperty("lowPrice").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var lVal) ? lVal : price * 0.95m;
                        decimal vol = decimal.TryParse(root.GetProperty("volume").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var vVal) ? vVal : 100000m;

                        var assetBase = binanceSym.EndsWith("USDT") ? binanceSym.Substring(0, binanceSym.Length - 4) : binanceSym;
                        var item = new TradingCatalogItem(binanceSym, $"{assetBase} / Tether USD", AssetClass.Crypto, price, pct, high, low, vol, IsSimulated: false, FeedSource: "binance", MaxLeverage: GetKnownMaxLeverage(binanceSym));
                        DynamicCatalog[binanceSym] = item;
                        LiveCache[binanceSym] = new LiveQuoteState(price, change, pct, high, low, vol, IsSimulated: false);

                        results[binanceSym] = new TradingQuoteDto(
                            binanceSym,
                            item.Name,
                            AssetClass.Crypto,
                            price,
                            change,
                            pct,
                            high,
                            low,
                            vol,
                            watchlistedSymbols.Contains(binanceSym),
                            IsSimulated: false,
                            FeedSource: "binance",
                            MaxLeverage: item.MaxLeverage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Binance live search query failed for {Query}", cleanQ);
            }
        }

        // 3.5 Live search via MEXC Public 24hr Ticker API (Tier-2 authoritative exchange for tokens not listed on Binance, e.g. AGTUSDT)
        if (cleanQ.Length >= 2 && !results.ContainsKey(targetSym))
        {
            try
            {
                var mexcRes = await _http.GetAsync($"https://api.mexc.com/api/v3/ticker/24hr?symbol={targetSym}", ct);
                if (mexcRes.IsSuccessStatusCode)
                {
                    var json = await mexcRes.Content.ReadAsStringAsync(ct);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("lastPrice", out var lpProp) &&
                        decimal.TryParse(lpProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price) &&
                        price > 0)
                    {
                        decimal change = decimal.TryParse(root.GetProperty("priceChange").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var cVal) ? cVal : 0m;
                        decimal pct = decimal.TryParse(root.GetProperty("priceChangePercent").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var pVal) ? pVal * 100m : 0m;
                        decimal high = decimal.TryParse(root.GetProperty("highPrice").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var hVal) ? hVal : price * 1.05m;
                        decimal low = decimal.TryParse(root.GetProperty("lowPrice").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var lVal) ? lVal : price * 0.95m;
                        decimal vol = decimal.TryParse(root.GetProperty("volume").GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var vVal) ? vVal : 100000m;

                        var assetBase = targetSym.EndsWith("USDT") ? targetSym.Substring(0, targetSym.Length - 4) : targetSym;
                        var item = new TradingCatalogItem(targetSym, $"{assetBase} / Tether USD", AssetClass.Crypto, price, pct, high, low, vol, IsSimulated: false, FeedSource: "mexc", MaxLeverage: GetKnownMaxLeverage(targetSym));
                        DynamicCatalog[targetSym] = item;
                        LiveCache[targetSym] = new LiveQuoteState(price, change, pct, high, low, vol, IsSimulated: false);

                        results[targetSym] = new TradingQuoteDto(
                            targetSym,
                            item.Name,
                            AssetClass.Crypto,
                            price,
                            change,
                            pct,
                            high,
                            low,
                            vol,
                            watchlistedSymbols.Contains(targetSym),
                            IsSimulated: false,
                            FeedSource: "mexc",
                            MaxLeverage: item.MaxLeverage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "MEXC live search query failed for {Query}", targetSym);
            }
        }

        // 4. If still no match and query looks like a ticker, generate simulated instrument
        if (results.Count == 0 && cleanQ.Length >= 2 && cleanQ.Length <= 12)
        {
            var sym = cleanQ.EndsWith("USDT") ? cleanQ : cleanQ + "USDT";
            var price = 10.00m;
            var item = new TradingCatalogItem(sym, $"{cleanQ} (Custom Asset)", AssetClass.Crypto, price, 0m, price * 1.02m, price * 0.98m, 500000m, IsSimulated: true, FeedSource: "simulated", MaxLeverage: GetKnownMaxLeverage(sym));
            DynamicCatalog[sym] = item;
            LiveCache[sym] = new LiveQuoteState(price, 0m, 0m, price * 1.02m, price * 0.98m, 500000m, IsSimulated: true);

            results[sym] = new TradingQuoteDto(
                sym,
                item.Name,
                AssetClass.Crypto,
                price,
                0m,
                0m,
                price * 1.02m,
                price * 0.98m,
                500000m,
                watchlistedSymbols.Contains(sym),
                IsSimulated: true,
                MaxLeverage: item.MaxLeverage);
        }

        return results.Values.ToList();
    }

    public IReadOnlyList<TradingQuoteDto> GetAllQuotes(HashSet<string>? watchlistedSymbols = null)
    {
        watchlistedSymbols ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var allItems = Catalog.Concat(DynamicCatalog.Values);
        return allItems.Select(c =>
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
                    live.IsSimulated,
                    c.FeedSource,
                    c.MaxLeverage
                );
            }

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
                c.IsSimulated,
                c.FeedSource,
                c.MaxLeverage
            );
        }).ToList();
    }

    public TradingQuoteDto? GetQuote(string symbol, HashSet<string>? watchlistedSymbols = null)
    {
        var item = Catalog.FirstOrDefault(c => c.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase))
                   ?? (DynamicCatalog.TryGetValue(symbol, out var dynamicItem) ? dynamicItem : null);

        if (item == null) return null;

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
                live.IsSimulated,
                item.FeedSource,
                item.MaxLeverage
            );
        }

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
            item.IsSimulated,
            item.FeedSource,
            item.MaxLeverage
        );
    }

    public decimal GetCurrentPrice(string symbol, decimal fallback = 100m)
    {
        var cleanSym = (symbol ?? string.Empty).Trim().ToUpperInvariant();
        if (LiveCache.TryGetValue(cleanSym, out var live) && live.Price > 0)
        {
            return live.Price;
        }

        var item = Catalog.FirstOrDefault(c => c.Symbol.Equals(cleanSym, StringComparison.OrdinalIgnoreCase))
                   ?? (DynamicCatalog.TryGetValue(cleanSym, out var dynamicItem) ? dynamicItem : null);

        return item != null ? item.BasePrice : fallback;
    }

    public async Task<int> GetMaxLeverageAsync(string symbol, CancellationToken ct = default)
    {
        var bracket = await GetLeverageBracketAsync(symbol, ct);
        return bracket.MaxLeverage;
    }

    public int GetMaxLeverage(string symbol)
    {
        var bracket = GetLeverageBracket(symbol);
        return bracket.MaxLeverage;
    }

    public TradingLeverageBracketDto GetLeverageBracket(string symbol)
    {
        var cleanSym = NormalizeBinanceSymbol(symbol);
        if (LeverageBracketCache.TryGetValue(cleanSym, out var cached) && cached.ExpiresAtUtc > DateTime.UtcNow)
        {
            return cached.Data;
        }

        var fallbackMax = GetKnownMaxLeverage(cleanSym);
        var tiers = CreateFallbackTiers(cleanSym, fallbackMax);
        var dto = new TradingLeverageBracketDto(cleanSym, fallbackMax, 1, tiers);
        LeverageBracketCache[cleanSym] = (dto, DateTime.UtcNow.AddMinutes(10));
        return dto;
    }

    public async Task<TradingLeverageBracketDto> GetLeverageBracketAsync(string symbol, CancellationToken ct = default)
    {
        var cleanSym = NormalizeBinanceSymbol(symbol);
        if (LeverageBracketCache.TryGetValue(cleanSym, out var cached) && cached.ExpiresAtUtc > DateTime.UtcNow)
        {
            return cached.Data;
        }

        // Live fetch via Binance Futures: GET https://fapi.binance.com/fapi/v1/leverageBracket?symbol=
        TradingLeverageBracketDto? live = null;
        try
        {
            live = await FetchBinanceLeverageBracketAsync(cleanSym, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Binance leverageBracket request failed for {Symbol}", cleanSym);
        }

        if (live != null)
        {
            LeverageBracketCache[cleanSym] = (live, DateTime.UtcNow.AddMinutes(30));
            UpdateCatalogMaxLeverage(cleanSym, live.MaxLeverage);
            return live;
        }

        // Graceful fallback to known crypto tiers or 1-250
        var fallbackMax = GetKnownMaxLeverage(cleanSym);
        var tiers = CreateFallbackTiers(cleanSym, fallbackMax);
        var fallbackDto = new TradingLeverageBracketDto(cleanSym, fallbackMax, 1, tiers);
        LeverageBracketCache[cleanSym] = (fallbackDto, DateTime.UtcNow.AddMinutes(10));
        UpdateCatalogMaxLeverage(cleanSym, fallbackMax);
        return fallbackDto;
    }

    private async Task<TradingLeverageBracketDto?> FetchBinanceLeverageBracketAsync(string cleanSym, CancellationToken ct)
    {
        if (!cleanSym.EndsWith("USDT", StringComparison.OrdinalIgnoreCase) &&
            !cleanSym.EndsWith("BUSD", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var apiKey = _configuration?["Binance:ApiKey"] ?? string.Empty;
        var apiSecret = _configuration?["Binance:SecretKey"] ?? string.Empty;

        var url = $"https://fapi.binance.com/fapi/v1/leverageBracket?symbol={cleanSym}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiSecret))
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var queryString = $"symbol={cleanSym}&timestamp={timestamp}";
            var signature = ComputeHmacSha256(queryString, apiSecret);
            url = $"https://fapi.binance.com/fapi/v1/leverageBracket?{queryString}&signature={signature}";
            request.RequestUri = new Uri(url);
            request.Headers.Add("X-MBX-APIKEY", apiKey);
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(4));

        var response = await _http.SendAsync(request, cts.Token);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cts.Token);
        using var doc = JsonDocument.Parse(json);

        JsonElement bracketsArray;
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            if (doc.RootElement.GetArrayLength() == 0) return null;
            var symbolObj = doc.RootElement[0];
            if (!symbolObj.TryGetProperty("brackets", out bracketsArray)) return null;
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            if (!doc.RootElement.TryGetProperty("brackets", out bracketsArray)) return null;
        }
        else
        {
            return null;
        }

        var tiers = new List<TradingBracketTierDto>();
        foreach (var b in bracketsArray.EnumerateArray())
        {
            var bracket = b.TryGetProperty("bracket", out var brProp) ? brProp.GetInt32() : tiers.Count + 1;
            var initialLev = b.TryGetProperty("initialLeverage", out var levProp) ? levProp.GetInt32() : 20;
            var notionalCap = b.TryGetProperty("notionalCap", out var nCap) && nCap.TryGetDecimal(out var nc) ? nc : 0m;
            var notionalFloor = b.TryGetProperty("notionalFloor", out var nFloor) && nFloor.TryGetDecimal(out var nf) ? nf : 0m;
            var maintMargin = b.TryGetProperty("maintMarginRatio", out var mmr) && mmr.TryGetDecimal(out var mr) ? mr : 0m;
            var cum = b.TryGetProperty("cum", out var cProp) && cProp.TryGetDecimal(out var cVal) ? cVal : 0m;

            tiers.Add(new TradingBracketTierDto(bracket, initialLev, notionalCap, notionalFloor, maintMargin, cum));
        }

        if (tiers.Count == 0) return null;

        var maxLeverage = tiers.Max(t => t.InitialLeverage);
        if (maxLeverage <= 0) maxLeverage = 250;

        return new TradingLeverageBracketDto(cleanSym, maxLeverage, 1, tiers);
    }

    private static IReadOnlyList<TradingBracketTierDto> CreateFallbackTiers(string symbol, int maxLev)
    {
        var tiers = new List<TradingBracketTierDto>();
        tiers.Add(new TradingBracketTierDto(1, maxLev, 50000m, 0m, 0.005m, 0m));
        if (maxLev > 50)
            tiers.Add(new TradingBracketTierDto(2, 50, 250000m, 50000m, 0.01m, 250m));
        if (maxLev > 20)
            tiers.Add(new TradingBracketTierDto(3, 20, 1000000m, 250000m, 0.025m, 4000m));
        if (maxLev > 10)
            tiers.Add(new TradingBracketTierDto(4, 10, 5000000m, 1000000m, 0.05m, 29000m));
        if (maxLev > 5)
            tiers.Add(new TradingBracketTierDto(5, 5, 10000000m, 5000000m, 0.10m, 279000m));
        tiers.Add(new TradingBracketTierDto(tiers.Count + 1, 1, 50000000m, 10000000m, 0.50m, 4279000m));
        return tiers;
    }

    private static int GetKnownMaxLeverage(string cleanSym)
    {
        var item = Catalog.FirstOrDefault(c => c.Symbol.Equals(cleanSym, StringComparison.OrdinalIgnoreCase))
                   ?? (DynamicCatalog.TryGetValue(cleanSym, out var dyn) ? dyn : null);

        if (item != null && item.MaxLeverage > 0)
        {
            return item.MaxLeverage;
        }

        return cleanSym switch
        {
            "BTCUSDT" => 125,
            "ETHUSDT" => 100,
            "SOLUSDT" or "BNBUSDT" or "XRPUSDT" or "DOGEUSDT" or "ADAUSDT" or "LINKUSDT" => 75,
            "AVAXUSDT" or "OPUSDT" or "SUIUSDT" or "NEARUSDT" or "PEPEUSDT" or "ARBUSDT" or "APTUSDT" or "RENDERUSDT" or "FETUSDT" => 50,
            _ => 250
        };
    }

    private static void UpdateCatalogMaxLeverage(string symbol, int maxLeverage)
    {
        var cat = Catalog.FirstOrDefault(c => c.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        if (cat != null)
        {
            var idx = Catalog.IndexOf(cat);
            if (idx >= 0)
            {
                Catalog[idx] = cat with { MaxLeverage = maxLeverage };
            }
        }
        else if (DynamicCatalog.TryGetValue(symbol, out var dyn))
        {
            DynamicCatalog[symbol] = dyn with { MaxLeverage = maxLeverage };
        }
    }

    private static string NormalizeBinanceSymbol(string symbol)
    {
        var clean = (symbol ?? string.Empty).Trim().ToUpperInvariant().Replace("/", "").Replace("-", "");
        if (!clean.EndsWith("USDT") && !clean.EndsWith("BUSD") && !clean.EndsWith("EUR") && !clean.EndsWith("USD"))
        {
            clean += "USDT";
        }
        return clean;
    }

    private static string ComputeHmacSha256(string message, string secret)
    {
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(secret);
        var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
        using var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(messageBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

internal static class TradingMarketDataExtensions
{
    public static int PricePrecision(this TradingCatalogItem item)
    {
        if (item.AssetClass == AssetClass.Forex) return 5;
        if (item.BasePrice < 0.0001m) return 8;
        if (item.BasePrice < 0.01m) return 6;
        if (item.BasePrice < 1.0m) return 5;
        if (item.BasePrice < 10.0m) return 4;
        if (item.AssetClass == AssetClass.Stock) return 4;
        return 2;
    }
}
