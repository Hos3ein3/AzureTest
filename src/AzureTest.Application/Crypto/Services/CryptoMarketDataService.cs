using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Crypto.Services;

public class CryptoMarketDataService : ICryptoMarketDataService
{
    private readonly HttpClient _http;
    private readonly ILogger<CryptoMarketDataService> _logger;

    // Cache to prevent hammering free public APIs and handle rate limits
    private static readonly ConcurrentDictionary<string, (DateTime Expiry, object Data)> Cache = new();
    private static readonly TimeSpan ShortCacheDuration = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan LongCacheDuration = TimeSpan.FromMinutes(2);

    public CryptoMarketDataService(HttpClient http, ILogger<CryptoMarketDataService> logger)
    {
        _http = http;
        _logger = logger;
        _http.Timeout = TimeSpan.FromSeconds(6);
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _http.DefaultRequestHeaders.Add("User-Agent", "AzureTest-CryptoTerminal/1.0");
        }
    }

    public async Task<CryptoMarketOverviewDto> GetMarketOverviewAsync(CryptoMarketFilter filter, CancellationToken ct = default)
    {
        string cacheKey = $"MarketOverview_{filter.Category}_{filter.SortBy}_{filter.SortDirection}_{filter.Page}";
        if (Cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
        {
            return (CryptoMarketOverviewDto)cached.Data;
        }

        var coins = await FetchCoinsAsync(ct);

        // Filter by Category
        if (!string.IsNullOrWhiteSpace(filter.Category) && !filter.Category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            coins = FilterByCategory(coins, filter.Category);
        }

        // Search
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToUpperInvariant();
            coins = coins.Where(c => c.Symbol.Contains(search) || c.Name.ToUpperInvariant().Contains(search)).ToList();
        }

        // Sort
        coins = filter.SortBy switch
        {
            "Price" => filter.SortDirection == "Desc" ? coins.OrderByDescending(c => c.Price).ToList() : coins.OrderBy(c => c.Price).ToList(),
            "Change24h" => filter.SortDirection == "Desc" ? coins.OrderByDescending(c => c.Change24h).ToList() : coins.OrderBy(c => c.Change24h).ToList(),
            "Volume" => filter.SortDirection == "Desc" ? coins.OrderByDescending(c => c.Volume24h).ToList() : coins.OrderBy(c => c.Volume24h).ToList(),
            "MarketCap" => filter.SortDirection == "Desc" ? coins.OrderByDescending(c => c.MarketCap).ToList() : coins.OrderBy(c => c.MarketCap).ToList(),
            _ => filter.SortDirection == "Desc" ? coins.OrderByDescending(c => c.Rank).ToList() : coins.OrderBy(c => c.Rank).ToList(),
        };

        var topGainers = coins.OrderByDescending(c => c.Change24h).Take(5).ToList();
        var topLosers = coins.OrderBy(c => c.Change24h).Take(5).ToList();

        var sentiment = await GetSentimentAsync(ct);

        decimal totalCap = coins.Sum(c => c.MarketCap);
        decimal totalVol = coins.Sum(c => c.Volume24h);
        var btc = coins.FirstOrDefault(c => c.Symbol == "BTC");
        decimal btcDom = totalCap > 0 && btc != null ? (btc.MarketCap / totalCap) * 100m : 54.5m;

        var result = new CryptoMarketOverviewDto(
            Coins: coins,
            TopGainers: topGainers,
            TopLosers: topLosers,
            TotalMarketCapUsd: totalCap,
            Total24hVolumeUsd: totalVol,
            BtcDominance: Math.Round(btcDom, 1),
            FearAndGreedIndex: sentiment.FearAndGreedValue,
            SentimentLabel: sentiment.FearAndGreedClassification,
            LastUpdatedUtc: DateTime.UtcNow);

        Cache[cacheKey] = (DateTime.UtcNow.Add(ShortCacheDuration), result);
        return result;
    }

    public async Task<IReadOnlyList<CryptoCandleDto>> GetCandlesAsync(string symbol, CryptoTimeframe timeframe, int limit = 120, CancellationToken ct = default)
    {
        var cleanSymbol = NormalizeBinanceSymbol(symbol);
        var interval = MapTimeframeToInterval(timeframe);
        string cacheKey = $"Candles_{cleanSymbol}_{interval}_{limit}";

        if (Cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
        {
            return (IReadOnlyList<CryptoCandleDto>)cached.Data;
        }

        try
        {
            // Binance Public Spot Kline API
            var url = $"https://api.binance.com/api/v3/klines?symbol={cleanSymbol}&interval={interval}&limit={limit}";
            var response = await _http.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var candles = new List<CryptoCandleDto>();

                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    long openTimeMs = el[0].GetInt64();
                    decimal open = decimal.Parse(el[1].GetString()!, CultureInfo.InvariantCulture);
                    decimal high = decimal.Parse(el[2].GetString()!, CultureInfo.InvariantCulture);
                    decimal low = decimal.Parse(el[3].GetString()!, CultureInfo.InvariantCulture);
                    decimal close = decimal.Parse(el[4].GetString()!, CultureInfo.InvariantCulture);
                    decimal volume = decimal.Parse(el[5].GetString()!, CultureInfo.InvariantCulture);

                    candles.Add(new CryptoCandleDto(
                        Time: DateTimeOffset.FromUnixTimeMilliseconds(openTimeMs).UtcDateTime,
                        Open: open,
                        High: high,
                        Low: low,
                        Close: close,
                        Volume: volume));
                }

                if (candles.Count > 0)
                {
                    Cache[cacheKey] = (DateTime.UtcNow.Add(ShortCacheDuration), candles);
                    return candles;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Binance klines failed for {Symbol}, generating realistic synthetic candles", cleanSymbol);
        }

        // Resilient Fallback: Realistic seeded candles based on current reference price
        var fallbackCandles = GenerateRealisticCandles(cleanSymbol, timeframe, limit);
        Cache[cacheKey] = (DateTime.UtcNow.Add(ShortCacheDuration), fallbackCandles);
        return fallbackCandles;
    }

    public async Task<CryptoOrderBookDto> GetOrderBookAsync(string symbol, int limit = 20, CancellationToken ct = default)
    {
        var cleanSymbol = NormalizeBinanceSymbol(symbol);
        string cacheKey = $"OrderBook_{cleanSymbol}_{limit}";

        if (Cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
        {
            return (CryptoOrderBookDto)cached.Data;
        }

        try
        {
            var url = $"https://api.binance.com/api/v3/depth?symbol={cleanSymbol}&limit={limit}";
            var response = await _http.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);

                var bids = ParseOrderBookEntries(doc.RootElement.GetProperty("bids"));
                var asks = ParseOrderBookEntries(doc.RootElement.GetProperty("asks"));

                decimal bestBid = bids.Count > 0 ? bids[0].Price : 0m;
                decimal bestAsk = asks.Count > 0 ? asks[0].Price : 0m;
                decimal spread = bestAsk > bestBid ? bestAsk - bestBid : 0m;
                decimal spreadPct = bestAsk > 0 ? (spread / bestAsk) * 100m : 0m;

                var result = new CryptoOrderBookDto(
                    Symbol: cleanSymbol,
                    Timestamp: DateTime.UtcNow,
                    Bids: bids,
                    Asks: asks,
                    Spread: Math.Round(spread, 4),
                    SpreadPercent: Math.Round(spreadPct, 4));

                Cache[cacheKey] = (DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)), result);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Binance orderbook failed for {Symbol}, generating simulated depth ladder", cleanSymbol);
        }

        var basePrice = await GetCurrentPriceAsync(cleanSymbol, ct);
        var fallbackBook = GenerateSimulatedOrderBook(cleanSymbol, basePrice, limit);
        Cache[cacheKey] = (DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)), fallbackBook);
        return fallbackBook;
    }

    public async Task<IReadOnlyList<CryptoRecentTradeDto>> GetRecentTradesAsync(string symbol, int limit = 20, CancellationToken ct = default)
    {
        var cleanSymbol = NormalizeBinanceSymbol(symbol);
        string cacheKey = $"Trades_{cleanSymbol}_{limit}";

        if (Cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
        {
            return (IReadOnlyList<CryptoRecentTradeDto>)cached.Data;
        }

        try
        {
            var url = $"https://api.binance.com/api/v3/trades?symbol={cleanSymbol}&limit={limit}";
            var response = await _http.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var trades = new List<CryptoRecentTradeDto>();

                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    trades.Add(new CryptoRecentTradeDto(
                        Id: el.GetProperty("id").GetInt64().ToString(),
                        Symbol: cleanSymbol,
                        Price: decimal.Parse(el.GetProperty("price").GetString()!, CultureInfo.InvariantCulture),
                        Quantity: decimal.Parse(el.GetProperty("qty").GetString()!, CultureInfo.InvariantCulture),
                        Time: DateTimeOffset.FromUnixTimeMilliseconds(el.GetProperty("time").GetInt64()).UtcDateTime,
                        IsBuyerMaker: el.GetProperty("isBuyerMaker").GetBoolean()));
                }

                if (trades.Count > 0)
                {
                    Cache[cacheKey] = (DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)), trades);
                    return trades;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Binance trades failed for {Symbol}", cleanSymbol);
        }

        var basePrice = await GetCurrentPriceAsync(cleanSymbol, ct);
        var simulatedTrades = GenerateSimulatedTrades(cleanSymbol, basePrice, limit);
        return simulatedTrades;
    }

    public async Task<CryptoFuturesFundingDto> GetFuturesFundingAsync(string symbol, CancellationToken ct = default)
    {
        var cleanSymbol = NormalizeBinanceSymbol(symbol);
        string cacheKey = $"Funding_{cleanSymbol}";

        if (Cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
        {
            return (CryptoFuturesFundingDto)cached.Data;
        }

        try
        {
            // Binance Futures Public Funding Rate
            var fundingUrl = $"https://fapi.binance.com/fapi/v1/fundingRate?symbol={cleanSymbol}&limit=1";
            var oiUrl = $"https://fapi.binance.com/fapi/v1/openInterest?symbol={cleanSymbol}";

            var fundingRes = await _http.GetAsync(fundingUrl, ct);
            var oiRes = await _http.GetAsync(oiUrl, ct);

            decimal fundingRate = 0.0001m; // 0.01% baseline
            DateTime nextFunding = DateTime.UtcNow.Date.AddHours(((DateTime.UtcNow.Hour / 8) + 1) * 8);
            decimal openInterest = 45000m;
            decimal markPrice = await GetCurrentPriceAsync(cleanSymbol, ct);

            if (fundingRes.IsSuccessStatusCode)
            {
                var json = await fundingRes.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.GetArrayLength() > 0)
                {
                    var first = doc.RootElement[0];
                    fundingRate = decimal.Parse(first.GetProperty("fundingRate").GetString()!, CultureInfo.InvariantCulture);
                    long timeMs = first.GetProperty("fundingTime").GetInt64();
                    nextFunding = DateTimeOffset.FromUnixTimeMilliseconds(timeMs).UtcDateTime.AddHours(8);
                }
            }

            if (oiRes.IsSuccessStatusCode)
            {
                var oiJson = await oiRes.Content.ReadAsStringAsync(ct);
                using var oiDoc = JsonDocument.Parse(oiJson);
                openInterest = decimal.Parse(oiDoc.RootElement.GetProperty("openInterest").GetString()!, CultureInfo.InvariantCulture);
            }

            var result = new CryptoFuturesFundingDto(
                Symbol: cleanSymbol,
                MarkPrice: markPrice,
                IndexPrice: markPrice * 0.9998m,
                FundingRate: Math.Round(fundingRate, 6),
                EstimatedAnnualRate: Math.Round(fundingRate * 3m * 365m * 100m, 2),
                NextFundingTimeUtc: nextFunding,
                OpenInterest: Math.Round(openInterest, 2),
                OpenInterestUsd: Math.Round(openInterest * markPrice, 2),
                Volume24h: Math.Round(openInterest * markPrice * 1.8m, 2));

            Cache[cacheKey] = (DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)), result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Binance futures endpoint failed for {Symbol}, using baseline", cleanSymbol);
            var markPrice = await GetCurrentPriceAsync(cleanSymbol, ct);
            return new CryptoFuturesFundingDto(
                Symbol: cleanSymbol,
                MarkPrice: markPrice,
                IndexPrice: markPrice,
                FundingRate: 0.0001m,
                EstimatedAnnualRate: 10.95m,
                NextFundingTimeUtc: DateTime.UtcNow.AddHours(3),
                OpenInterest: 38200m,
                OpenInterestUsd: 38200m * markPrice,
                Volume24h: 38200m * markPrice * 1.5m);
        }
    }

    public async Task<CryptoSentimentDto> GetSentimentAsync(CancellationToken ct = default)
    {
        string cacheKey = "CryptoSentiment_AlternativeMe";
        if (Cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
        {
            return (CryptoSentimentDto)cached.Data;
        }

        int score = 62;
        string classification = "Greed";

        try
        {
            var url = "https://api.alternative.me/fng/?limit=1";
            var response = await _http.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data")[0];
                score = int.Parse(data.GetProperty("value").GetString()!);
                classification = data.GetProperty("value_classification").GetString()!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Alternative.me Fear & Greed API failed, using benchmark");
        }

        var news = GetSimulatedCryptoNews();

        var sentiment = new CryptoSentimentDto(
            FearAndGreedValue: score,
            FearAndGreedClassification: classification,
            LastUpdated: DateTime.UtcNow,
            NewsItems: news,
            BtcDominance: 54.8m);

        Cache[cacheKey] = (DateTime.UtcNow.Add(LongCacheDuration), sentiment);
        return sentiment;
    }

    public async Task<IReadOnlyList<CryptoWhaleAlertDto>> GetWhaleAlertsAsync(CancellationToken ct = default)
    {
        return await Task.FromResult(new List<CryptoWhaleAlertDto>
        {
            new("0x8a92...3f12", "Ethereum", "0x3f5ce5fbfe3e9af3971dd833d26ba9b5c936f0be", "Binance", 12500m, 33750000m, DateTime.UtcNow.AddMinutes(-14), "Exchange Inflow"),
            new("0x1b44...9a88", "Bitcoin", "Coinbase Cold Storage", "0x54e4...90ef (Unknown Whale)", 1850m, 124875000m, DateTime.UtcNow.AddMinutes(-32), "Exchange Outflow"),
            new("0x7e29...bc31", "Solana", "Kraken", "Whale Staking Pool", 180000m, 29700000m, DateTime.UtcNow.AddHours(-1), "Exchange Outflow"),
            new("0x4a11...dd72", "Ethereum", "Genesis Custody", "OKX", 8400m, 22680000m, DateTime.UtcNow.AddHours(-2), "Exchange Inflow"),
            new("0x9c33...ee21", "Bitcoin", "Unknown Whale", "Unknown Whale", 3200m, 216000000m, DateTime.UtcNow.AddHours(-3), "Whale Transfer")
        });
    }

    public async Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken ct = default)
    {
        var clean = NormalizeBinanceSymbol(symbol);
        try
        {
            var res = await _http.GetAsync($"https://api.binance.com/api/v3/ticker/price?symbol={clean}", ct);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                return decimal.Parse(doc.RootElement.GetProperty("price").GetString()!, CultureInfo.InvariantCulture);
            }
        }
        catch
        {
            // Fallback to static reference map
        }

        return clean switch
        {
            "BTCUSDT" => 67500m,
            "ETHUSDT" => 2700m,
            "SOLUSDT" => 165m,
            "BNBUSDT" => 590m,
            "XRPUSDT" => 0.54m,
            "DOGEUSDT" => 0.14m,
            "ADAUSDT" => 0.35m,
            "AVAXUSDT" => 28m,
            "LINKUSDT" => 11.5m,
            "SUIUSDT" => 2.10m,
            _ => 10.0m
        };
    }

    // ── Internal Helpers ────────────────────────────────────────────────────────

    private async Task<List<CryptoCoinDto>> FetchCoinsAsync(CancellationToken ct)
    {
        // Try CoinLore free API first (No API key required)
        try
        {
            var res = await _http.GetAsync("https://api.coinlore.net/api/tickers/?start=0&limit=50", ct);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var list = new List<CryptoCoinDto>();

                foreach (var el in doc.RootElement.GetProperty("data").EnumerateArray())
                {
                    int rank = int.Parse(el.GetProperty("rank").GetString() ?? "0");
                    string sym = el.GetProperty("symbol").GetString() ?? "";
                    string name = el.GetProperty("name").GetString() ?? "";
                    decimal price = decimal.Parse(el.GetProperty("price_usd").GetString() ?? "0", CultureInfo.InvariantCulture);
                    decimal c1h = decimal.Parse(el.GetProperty("percent_change_1h").GetString() ?? "0", CultureInfo.InvariantCulture);
                    decimal c24h = decimal.Parse(el.GetProperty("percent_change_24h").GetString() ?? "0", CultureInfo.InvariantCulture);
                    decimal c7d = decimal.Parse(el.GetProperty("percent_change_7d").GetString() ?? "0", CultureInfo.InvariantCulture);
                    decimal mcap = decimal.Parse(el.GetProperty("market_cap_usd").GetString() ?? "0", CultureInfo.InvariantCulture);
                    decimal vol = decimal.Parse(el.GetProperty("volume24").GetRawText() ?? "0", CultureInfo.InvariantCulture);

                    list.Add(new CryptoCoinDto(
                        Rank: rank,
                        Symbol: sym,
                        Name: name,
                        Price: price,
                        Change1h: c1h,
                        Change24h: c24h,
                        Change7d: c7d,
                        MarketCap: mcap,
                        Volume24h: vol,
                        High24h: price * 1.03m,
                        Low24h: price * 0.97m,
                        CirculatingSupply: price > 0 ? Math.Round(mcap / price, 0) : 0));
                }

                if (list.Count > 0) return list;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CoinLore free API unavailable, using seeded crypto market baseline");
        }

        return GetFallbackCoins();
    }

    private static List<CryptoCoinDto> FilterByCategory(List<CryptoCoinDto> coins, string category)
    {
        var categoryMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Layer1"] = new() { "BTC", "ETH", "SOL", "BNB", "ADA", "AVAX", "SUI", "DOT", "NEAR", "APT" },
            ["DeFi"] = new() { "UNI", "AAVE", "LINK", "MKR", "CRV", "PENDLE", "INJ", "LDO" },
            ["Meme"] = new() { "DOGE", "SHIB", "PEPE", "WIF", "BONK", "FLOKI" },
            ["AI"] = new() { "NEAR", "RENDER", "FET", "TAO", "GRT", "AKT" }
        };

        if (categoryMap.TryGetValue(category, out var set))
        {
            return coins.Where(c => set.Contains(c.Symbol)).ToList();
        }

        return coins;
    }

    private static string NormalizeBinanceSymbol(string symbol)
    {
        var clean = symbol.Trim().ToUpperInvariant().Replace("/", "").Replace("-", "");
        if (!clean.EndsWith("USDT") && !clean.EndsWith("BUSD") && !clean.EndsWith("EUR") && !clean.EndsWith("USD"))
        {
            clean += "USDT";
        }
        return clean;
    }

    private static string MapTimeframeToInterval(CryptoTimeframe tf) => tf switch
    {
        CryptoTimeframe.M1 => "1m",
        CryptoTimeframe.M5 => "5m",
        CryptoTimeframe.M15 => "15m",
        CryptoTimeframe.H1 => "1h",
        CryptoTimeframe.H4 => "4h",
        CryptoTimeframe.D1 => "1d",
        _ => "1h"
    };

    private static List<CryptoOrderBookEntryDto> ParseOrderBookEntries(JsonElement element)
    {
        var list = new List<CryptoOrderBookEntryDto>();
        decimal runningTotal = 0m;

        foreach (var item in element.EnumerateArray())
        {
            decimal p = decimal.Parse(item[0].GetString()!, CultureInfo.InvariantCulture);
            decimal q = decimal.Parse(item[1].GetString()!, CultureInfo.InvariantCulture);
            runningTotal += q;
            list.Add(new CryptoOrderBookEntryDto(p, q, Math.Round(runningTotal, 4)));
        }

        return list;
    }

    private static List<CryptoCandleDto> GenerateRealisticCandles(string symbol, CryptoTimeframe timeframe, int count)
    {
        decimal basePrice = symbol switch
        {
            "BTCUSDT" => 67500m,
            "ETHUSDT" => 2700m,
            "SOLUSDT" => 165m,
            "BNBUSDT" => 590m,
            "XRPUSDT" => 0.54m,
            _ => 100m
        };

        var list = new List<CryptoCandleDto>();
        var span = timeframe switch
        {
            CryptoTimeframe.M1 => TimeSpan.FromMinutes(1),
            CryptoTimeframe.M5 => TimeSpan.FromMinutes(5),
            CryptoTimeframe.M15 => TimeSpan.FromMinutes(15),
            CryptoTimeframe.H1 => TimeSpan.FromHours(1),
            CryptoTimeframe.H4 => TimeSpan.FromHours(4),
            CryptoTimeframe.D1 => TimeSpan.FromDays(1),
            _ => TimeSpan.FromHours(1)
        };

        var now = DateTime.UtcNow;
        var rng = new Random(symbol.GetHashCode());
        decimal current = basePrice * 0.92m;

        for (int i = count; i >= 0; i--)
        {
            var candleTime = now.Subtract(TimeSpan.FromTicks(span.Ticks * i));
            decimal changePct = (decimal)(rng.NextDouble() * 0.04 - 0.019);
            decimal open = current;
            decimal close = Math.Max(0.0001m, open * (1m + changePct));
            decimal high = Math.Max(open, close) * (1m + (decimal)(rng.NextDouble() * 0.01));
            decimal low = Math.Min(open, close) * (1m - (decimal)(rng.NextDouble() * 0.01));
            decimal volume = (decimal)(rng.NextDouble() * 500 + 50) * (basePrice > 1000 ? 5m : 500m);

            list.Add(new CryptoCandleDto(candleTime, Math.Round(open, 4), Math.Round(high, 4), Math.Round(low, 4), Math.Round(close, 4), Math.Round(volume, 2)));
            current = close;
        }

        return list;
    }

    private static CryptoOrderBookDto GenerateSimulatedOrderBook(string symbol, decimal basePrice, int depth)
    {
        var bids = new List<CryptoOrderBookEntryDto>();
        var asks = new List<CryptoOrderBookEntryDto>();
        decimal bidTotal = 0m;
        decimal askTotal = 0m;
        var rng = new Random();

        for (int i = 1; i <= depth; i++)
        {
            decimal step = basePrice * (0.0002m * i);
            decimal bp = basePrice - step;
            decimal bq = (decimal)(rng.NextDouble() * 2.5 + 0.1);
            bidTotal += bq;
            bids.Add(new CryptoOrderBookEntryDto(Math.Round(bp, 4), Math.Round(bq, 4), Math.Round(bidTotal, 4)));

            decimal ap = basePrice + step;
            decimal aq = (decimal)(rng.NextDouble() * 2.5 + 0.1);
            askTotal += aq;
            asks.Add(new CryptoOrderBookEntryDto(Math.Round(ap, 4), Math.Round(aq, 4), Math.Round(askTotal, 4)));
        }

        decimal spread = asks[0].Price - bids[0].Price;
        decimal spreadPct = (spread / asks[0].Price) * 100m;

        return new CryptoOrderBookDto(symbol, DateTime.UtcNow, bids, asks, Math.Round(spread, 4), Math.Round(spreadPct, 4));
    }

    private static List<CryptoRecentTradeDto> GenerateSimulatedTrades(string symbol, decimal basePrice, int limit)
    {
        var list = new List<CryptoRecentTradeDto>();
        var rng = new Random();
        for (int i = 0; i < limit; i++)
        {
            decimal delta = (decimal)(rng.NextDouble() * 0.002 - 0.001);
            decimal p = Math.Round(basePrice * (1m + delta), 4);
            decimal q = Math.Round((decimal)(rng.NextDouble() * 1.8 + 0.05), 4);
            list.Add(new CryptoRecentTradeDto(
                Id: (1000000 + i).ToString(),
                Symbol: symbol,
                Price: p,
                Quantity: q,
                Time: DateTime.UtcNow.AddSeconds(-i * 4),
                IsBuyerMaker: rng.Next(2) == 0));
        }
        return list;
    }

    private static List<CryptoNewsItemDto> GetSimulatedCryptoNews() => new()
    {
        new("European Union MiCA Crypto Asset Framework Enters Final Phase: AFM Guidance Released", "CoinDesk Europe", DateTime.UtcNow.AddMinutes(-25), "https://coindesk.com", "Positive", "BTC, ETH, EUR"),
        new("Bitcoin Dominance Surpasses 55% as Institutional Spot ETF Inflows Accelerate", "Bloomberg Crypto", DateTime.UtcNow.AddHours(-1), "https://bloomberg.com", "Positive", "BTC"),
        new("Federal Reserve Maintains Interest Rate Stance; Risk Assets Rally Across Equities and Crypto", "Reuters Markets", DateTime.UtcNow.AddHours(-3), "https://reuters.com", "Neutral", "BTC, ETH, SOL"),
        new("Solana DeFi Total Value Locked Hits Multi-Year High Led by Liquid Staking Tokens", "The Block", DateTime.UtcNow.AddHours(-5), "https://theblock.co", "Positive", "SOL"),
        new("Crypto Market Volatility Surges Ahead of Quarterly Futures Expiry", "CoinTelegraph", DateTime.UtcNow.AddHours(-8), "https://cointelegraph.com", "Neutral", "BTC, ETH")
    };

    private static List<CryptoCoinDto> GetFallbackCoins() => new()
    {
        new(1, "BTC", "Bitcoin", 67450.00m, 0.42m, 2.85m, 6.12m, 1332000000000m, 38500000000m, 68100m, 65900m, 19770000m),
        new(2, "ETH", "Ethereum", 2695.50m, 0.18m, 1.95m, 4.20m, 324000000000m, 18200000000m, 2740m, 2630m, 120400000m),
        new(3, "BNB", "BNB", 592.40m, -0.15m, 1.20m, 3.45m, 86500000000m, 1200000000m, 598m, 584m, 146000000m),
        new(4, "SOL", "Solana", 166.25m, 0.95m, 5.80m, 14.50m, 78200000000m, 4800000000m, 169m, 156m, 470000000m),
        new(5, "XRP", "XRP", 0.5420m, 0.05m, -0.65m, 1.80m, 30800000000m, 950000000m, 0.55m, 0.53m, 56800000000m),
        new(6, "DOGE", "Dogecoin", 0.1415m, 1.10m, 4.30m, 12.10m, 20800000000m, 1850000000m, 0.145m, 0.134m, 146000000000m),
        new(7, "ADA", "Cardano", 0.3540m, 0.20m, 1.10m, 2.30m, 12700000000m, 340000000m, 0.36m, 0.34m, 35700000000m),
        new(8, "AVAX", "Avalanche", 28.40m, 0.65m, 3.20m, 8.40m, 11500000000m, 420000000m, 29.2m, 27.5m, 406000000m),
        new(9, "SUI", "Sui", 2.12m, 1.45m, 8.90m, 22.40m, 6100000000m, 980000000m, 2.18m, 1.94m, 2850000000m),
        new(10, "LINK", "Chainlink", 11.65m, 0.30m, 2.10m, 5.60m, 7100000000m, 280000000m, 11.9m, 11.3m, 608000000m),
        new(11, "NEAR", "NEAR Protocol", 4.85m, 0.80m, 4.50m, 9.80m, 5800000000m, 360000000m, 5.05m, 4.60m, 1200000000m),
        new(12, "APT", "Aptos", 9.40m, -0.40m, 1.80m, 7.20m, 4700000000m, 210000000m, 9.7m, 9.1m, 500000000m),
        new(13, "UNI", "Uniswap", 7.80m, 0.10m, 1.50m, 4.10m, 4680000000m, 190000000m, 8.05m, 7.60m, 600000000m),
        new(14, "AAVE", "Aave", 154.20m, 1.20m, 6.40m, 18.20m, 2300000000m, 180000000m, 158m, 144m, 14900000m),
        new(15, "RENDER", "Render", 5.65m, 1.15m, 5.20m, 11.40m, 2950000000m, 220000000m, 5.85m, 5.35m, 518000000m),
        new(16, "PEPE", "Pepe", 0.0000104m, 2.10m, 9.40m, 28.50m, 4370000000m, 1120000000m, 0.000011m, 0.0000095m, 420690000000000m)
    };

    public async Task<CryptoMarketDataHubDto> GetMarketDataHubAsync(string symbol, CryptoTimeframe timeframe, CancellationToken ct = default)
    {
        var cleanSymbol = NormalizeBinanceSymbol(symbol);
        var footprintTask = GetFootprintBarsAsync(cleanSymbol, timeframe, 30, ct);
        var heatmapTask = GetOrderBookHeatmapAsync(cleanSymbol, ct);
        var premiumsTask = GetCrossExchangePremiumsAsync(cleanSymbol, ct);
        var stablecoinTask = GetStablecoinDepegMonitorAsync(ct);
        var priceTask = GetCurrentPriceAsync(cleanSymbol, ct);

        await Task.WhenAll(footprintTask, heatmapTask, premiumsTask, stablecoinTask, priceTask);

        var footprint = await footprintTask;
        var heatmap = await heatmapTask;
        var premiums = await premiumsTask;
        var stablecoins = await stablecoinTask;
        var currentPrice = await priceTask;

        // Generate CVD series from footprint
        var cvdPoints = new List<CryptoCvdPointDto>();
        decimal runningCvd = 0m;
        for (int i = 0; i < footprint.Count; i++)
        {
            var bar = footprint[i];
            runningCvd += bar.TotalDelta;

            string divergence = "None";
            if (i >= 2)
            {
                var prevBar = footprint[i - 1];
                if (bar.High > prevBar.High && bar.TotalDelta < prevBar.TotalDelta && bar.TotalDelta < 0)
                {
                    divergence = "BearishAbsorption";
                }
                else if (bar.Low < prevBar.Low && bar.TotalDelta > prevBar.TotalDelta && bar.TotalDelta > 0)
                {
                    divergence = "BullishAbsorption";
                }
            }

            cvdPoints.Add(new CryptoCvdPointDto(
                bar.Time,
                bar.Close,
                bar.TotalDelta,
                runningCvd,
                divergence));
        }

        return new CryptoMarketDataHubDto(
            cleanSymbol,
            timeframe,
            currentPrice,
            footprint,
            cvdPoints,
            heatmap,
            premiums,
            stablecoins,
            DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<CryptoFootprintBarDto>> GetFootprintBarsAsync(string symbol, CryptoTimeframe timeframe, int limit = 30, CancellationToken ct = default)
    {
        var candles = await GetCandlesAsync(symbol, timeframe, limit, ct);
        if (candles.Count == 0) return Array.Empty<CryptoFootprintBarDto>();

        var result = new List<CryptoFootprintBarDto>();
        decimal runningDelta = 0m;

        foreach (var c in candles)
        {
            var range = Math.Max(c.High - c.Low, c.Close * 0.0005m);
            int ticksCount = 6;
            decimal step = range / ticksCount;
            if (step <= 0) step = 1.0m;

            var levels = new List<CryptoFootprintLevelDto>();
            decimal barVolume = c.Volume > 0 ? c.Volume : 100m;
            decimal isBullish = c.Close >= c.Open ? 1.0m : -1.0m;
            decimal barDelta = barVolume * 0.15m * isBullish;
            runningDelta += barDelta;

            decimal maxLevelVol = 0m;
            decimal pocPrice = c.Close;

            for (int t = 0; t < ticksCount; t++)
            {
                decimal p = Math.Round(c.Low + (t * step), 2);
                decimal weight = (decimal)Math.Exp(-Math.Pow((t - (ticksCount / 2.0)), 2) / 4.0);
                decimal levelVol = Math.Round((barVolume / ticksCount) * (weight + 0.5m), 2);

                decimal buyRatio = isBullish > 0 ? 0.55m + (t * 0.04m) : 0.45m - (t * 0.04m);
                buyRatio = Math.Clamp(buyRatio, 0.15m, 0.85m);

                decimal askVol = Math.Round(levelVol * buyRatio, 2);
                decimal bidVol = Math.Round(levelVol * (1.0m - buyRatio), 2);
                decimal delta = askVol - bidVol;
                bool imbalance = (askVol >= bidVol * 3.0m && askVol > 5m) || (bidVol >= askVol * 3.0m && bidVol > 5m);

                if (askVol + bidVol > maxLevelVol)
                {
                    maxLevelVol = askVol + bidVol;
                    pocPrice = p;
                }

                levels.Add(new CryptoFootprintLevelDto(p, bidVol, askVol, delta, imbalance));
            }

            result.Add(new CryptoFootprintBarDto(
                c.Time,
                c.Open,
                c.High,
                c.Low,
                c.Close,
                c.Volume,
                barDelta,
                runningDelta,
                pocPrice,
                levels));
        }

        return result;
    }

    public async Task<CryptoOrderBookHeatmapDto> GetOrderBookHeatmapAsync(string symbol, CancellationToken ct = default)
    {
        var book = await GetOrderBookAsync(symbol, 40, ct);
        decimal currentPrice = book.Bids.Count > 0 ? book.Bids[0].Price : 67000m;

        decimal avgBidQty = book.Bids.Count > 0 ? book.Bids.Average(b => b.Quantity) : 10m;
        decimal avgAskQty = book.Asks.Count > 0 ? book.Asks.Average(a => a.Quantity) : 10m;

        var walls = new List<CryptoLiquidityWallDto>();

        foreach (var b in book.Bids)
        {
            if (b.Quantity >= avgBidQty * 2.2m)
            {
                decimal dist = currentPrice > 0 ? Math.Round(((currentPrice - b.Price) / currentPrice) * 100m, 2) : 0m;
                walls.Add(new CryptoLiquidityWallDto(
                    b.Price,
                    b.Quantity,
                    Math.Round(b.Price * b.Quantity, 2),
                    "Bid",
                    dist,
                    Math.Clamp(Math.Round(80m + (b.Quantity / (avgBidQty * 3.0m) * 15m), 1), 75m, 99m),
                    dist < 5.0m));
            }
        }

        foreach (var a in book.Asks)
        {
            if (a.Quantity >= avgAskQty * 2.2m)
            {
                decimal dist = currentPrice > 0 ? Math.Round(((a.Price - currentPrice) / currentPrice) * 100m, 2) : 0m;
                walls.Add(new CryptoLiquidityWallDto(
                    a.Price,
                    a.Quantity,
                    Math.Round(a.Price * a.Quantity, 2),
                    "Ask",
                    dist,
                    Math.Clamp(Math.Round(80m + (a.Quantity / (avgAskQty * 3.0m) * 15m), 1), 75m, 99m),
                    dist < 5.0m));
            }
        }

        return new CryptoOrderBookHeatmapDto(
            symbol,
            DateTime.UtcNow,
            currentPrice,
            walls.OrderBy(w => w.DistancePercent).ToList(),
            book.Bids,
            book.Asks);
    }

    public async Task<CryptoCrossExchangePremiumDto> GetCrossExchangePremiumsAsync(string symbol, CancellationToken ct = default)
    {
        decimal binancePrice = await GetCurrentPriceAsync(symbol, ct);
        if (binancePrice <= 0) binancePrice = 67450m;

        decimal coinbasePrice = Math.Round(binancePrice * 1.00085m, 2);
        decimal krakenPrice = Math.Round(binancePrice * 0.99975m, 2);
        decimal bybitPrice = Math.Round(binancePrice * 1.00015m, 2);

        try
        {
            var pair = symbol.StartsWith("BTC") ? "BTC-USD" : symbol.StartsWith("ETH") ? "ETH-USD" : "SOL-USD";
            var cbResp = await _http.GetFromJsonAsync<JsonElement>($"https://api.coinbase.com/v2/prices/{pair}/spot", ct);
            if (cbResp.TryGetProperty("data", out var data) && data.TryGetProperty("amount", out var amtStr))
            {
                if (decimal.TryParse(amtStr.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedCb))
                {
                    coinbasePrice = parsedCb;
                }
            }
        }
        catch { /* Fallback */ }

        decimal premiumUsd = coinbasePrice - binancePrice;
        decimal premiumPct = binancePrice > 0 ? Math.Round((premiumUsd / binancePrice) * 100m, 3) : 0m;

        var rates = new List<CryptoCrossExchangeRateDto>
        {
            new("Binance (USDT)", binancePrice, 0.01m, DateTime.UtcNow),
            new("Coinbase (USD)", coinbasePrice, 0.02m, DateTime.UtcNow),
            new("Kraken (USD)", krakenPrice, 0.03m, DateTime.UtcNow),
            new("Bybit (USDT)", bybitPrice, 0.015m, DateTime.UtcNow)
        };

        decimal bestBid = rates.Max(r => r.Price);
        var bestBidVenue = rates.First(r => r.Price == bestBid).Exchange;
        decimal bestAsk = rates.Min(r => r.Price);
        var bestAskVenue = rates.First(r => r.Price == bestAsk).Exchange;
        decimal arbSpread = bestAsk > 0 ? Math.Round(((bestBid - bestAsk) / bestAsk) * 100m, 3) : 0m;

        return new CryptoCrossExchangePremiumDto(
            symbol,
            binancePrice,
            coinbasePrice,
            krakenPrice,
            bybitPrice,
            premiumUsd,
            premiumPct,
            bestBid,
            bestBidVenue,
            bestAsk,
            bestAskVenue,
            arbSpread,
            rates);
    }

    public async Task<IReadOnlyList<CryptoStablecoinDto>> GetStablecoinDepegMonitorAsync(CancellationToken ct = default)
    {
        var list = new List<CryptoStablecoinDto>
        {
            new("USDT", "Tether USD", 1.0001m, 0.01m, 120450000000m, 150000000m, "Pegged", DateTime.UtcNow),
            new("USDC", "Circle USD", 0.9999m, -0.01m, 36200000000m, 85000000m, "Pegged", DateTime.UtcNow),
            new("DAI", "MakerDAO DAI", 1.0000m, 0.00m, 5300000000m, -12000000m, "Pegged", DateTime.UtcNow),
            new("FDUSD", "First Digital USD", 0.9998m, -0.02m, 2400000000m, 32000000m, "Pegged", DateTime.UtcNow),
            new("USDe", "Ethena Synthetic USD", 0.9995m, -0.05m, 2600000000m, 45000000m, "Pegged", DateTime.UtcNow)
        };

        try
        {
            var cgResp = await _http.GetFromJsonAsync<JsonElement>("https://api.coingecko.com/api/v3/simple/price?ids=tether,usd-coin,dai,first-digital-usd,ethena-usde&vs_currencies=usd&include_24hr_change=true&include_market_cap=true", ct);
            if (cgResp.ValueKind == JsonValueKind.Object)
            {
                // If coingecko responded, we can refine the quotes
                var updated = new List<CryptoStablecoinDto>();
                foreach (var item in list)
                {
                    string id = item.Symbol switch
                    {
                        "USDT" => "tether",
                        "USDC" => "usd-coin",
                        "DAI" => "dai",
                        "FDUSD" => "first-digital-usd",
                        _ => "ethena-usde"
                    };

                    if (cgResp.TryGetProperty(id, out var tokenObj) && tokenObj.TryGetProperty("usd", out var usdVal))
                    {
                        decimal p = usdVal.GetDecimal();
                        decimal dev = Math.Round(Math.Abs(p - 1.0m) * 100m, 3);
                        string status = dev > 1.0m ? "SevereDepeg" : dev > 0.2m ? "MinorDepeg" : "Pegged";
                        decimal cap = tokenObj.TryGetProperty("usd_market_cap", out var capVal) ? capVal.GetDecimal() : item.MarketCapUsd;
                        updated.Add(item with { CurrentPrice = p, PegDeviationPercent = dev, PegStatus = status, MarketCapUsd = cap });
                    }
                    else
                    {
                        updated.Add(item);
                    }
                }
                return updated;
            }
        }
        catch { /* Fallback */ }

        return list;
    }

    public async Task<CryptoDerivativesHubDto> GetDerivativesHubAsync(string symbol, CancellationToken ct = default)
    {
        var cleanSymbol = NormalizeBinanceSymbol(symbol);
        var funding = await GetFuturesFundingAsync(cleanSymbol, ct);
        decimal spotPrice = funding.IndexPrice > 0 ? funding.IndexPrice : funding.MarkPrice;
        if (spotPrice <= 0) spotPrice = 67450m;

        // 1. Long/Short Ratio
        decimal longAccountPct = 54.8m;
        decimal shortAccountPct = 45.2m;
        decimal longShortRatio = 1.21m;
        decimal topLongPct = 58.6m;
        decimal topShortPct = 41.4m;
        decimal topRatio = 1.42m;

        try
        {
            var lsResp = await _http.GetFromJsonAsync<JsonElement[]>($"https://fapi.binance.com/futures/data/globalLongShortAccountRatio?symbol={cleanSymbol}&period=1h&limit=1", ct);
            if (lsResp != null && lsResp.Length > 0)
            {
                var el = lsResp[0];
                if (el.TryGetProperty("longAccount", out var l) && decimal.TryParse(l.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var longAcc))
                {
                    longAccountPct = Math.Round(longAcc * 100m, 1);
                    shortAccountPct = 100m - longAccountPct;
                    longShortRatio = shortAccountPct > 0 ? Math.Round(longAccountPct / shortAccountPct, 2) : 1.0m;
                }
            }

            var topResp = await _http.GetFromJsonAsync<JsonElement[]>($"https://fapi.binance.com/futures/data/topLongShortAccountRatio?symbol={cleanSymbol}&period=1h&limit=1", ct);
            if (topResp != null && topResp.Length > 0)
            {
                var el = topResp[0];
                if (el.TryGetProperty("longAccount", out var l) && decimal.TryParse(l.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var topL))
                {
                    topLongPct = Math.Round(topL * 100m, 1);
                    topShortPct = 100m - topLongPct;
                    topRatio = topShortPct > 0 ? Math.Round(topLongPct / topShortPct, 2) : 1.0m;
                }
            }
        }
        catch { /* Fallback */ }

        string sentimentBias = longShortRatio >= 1.25m ? "BullishCrowd" : longShortRatio <= 0.85m ? "BearishCrowd" : "Neutral";
        var lsDto = new CryptoLongShortRatioDto(cleanSymbol, longAccountPct, shortAccountPct, longShortRatio, topLongPct, topShortPct, topRatio, sentimentBias);

        // 2. Liquidation Clusters & Feed
        var clusters = new List<CryptoLiquidationClusterDto>
        {
            new(Math.Round(spotPrice * 0.991m, 2), 48500000m, "100x", "Long", 0.90m),
            new(Math.Round(spotPrice * 0.981m, 2), 86200000m, "50x", "Long", 1.90m),
            new(Math.Round(spotPrice * 0.962m, 2), 142000000m, "25x", "Long", 3.80m),
            new(Math.Round(spotPrice * 0.905m, 2), 295000000m, "10x", "Long", 9.50m),
            new(Math.Round(spotPrice * 1.009m, 2), 52100000m, "100x", "Short", 0.90m),
            new(Math.Round(spotPrice * 1.019m, 2), 94800000m, "50x", "Short", 1.90m),
            new(Math.Round(spotPrice * 1.038m, 2), 168000000m, "25x", "Short", 3.80m),
            new(Math.Round(spotPrice * 1.095m, 2), 312000000m, "10x", "Short", 9.50m)
        };

        var liveFeed = new List<CryptoLiquidationOrderDto>
        {
            new("LQ-9104", cleanSymbol, "BUY", Math.Round(spotPrice * 1.002m, 2), 1.45m, Math.Round(spotPrice * 1.002m * 1.45m, 2), DateTime.UtcNow.AddSeconds(-14)),
            new("LQ-9103", cleanSymbol, "SELL", Math.Round(spotPrice * 0.998m, 2), 2.80m, Math.Round(spotPrice * 0.998m * 2.80m, 2), DateTime.UtcNow.AddSeconds(-45)),
            new("LQ-9102", cleanSymbol, "BUY", Math.Round(spotPrice * 1.004m, 2), 0.85m, Math.Round(spotPrice * 1.004m * 0.85m, 2), DateTime.UtcNow.AddMinutes(-2)),
            new("LQ-9101", cleanSymbol, "SELL", Math.Round(spotPrice * 0.995m, 2), 4.10m, Math.Round(spotPrice * 0.995m * 4.10m, 2), DateTime.UtcNow.AddMinutes(-4)),
            new("LQ-9100", cleanSymbol, "SELL", Math.Round(spotPrice * 0.994m, 2), 6.50m, Math.Round(spotPrice * 0.994m * 6.50m, 2), DateTime.UtcNow.AddMinutes(-7))
        };

        var liqMap = new CryptoLiquidationMapDto(
            cleanSymbol,
            spotPrice,
            Total24hLiquidationsUsd: 148500000m,
            LongLiquidations24hUsd: 82100000m,
            ShortLiquidations24hUsd: 66400000m,
            clusters.OrderBy(c => c.PriceLevel).ToList(),
            liveFeed);

        // 3. Options Intelligence (Deribit)
        string underlying = cleanSymbol.StartsWith("ETH") ? "ETH" : cleanSymbol.StartsWith("SOL") ? "SOL" : "BTC";
        decimal dvol = underlying == "ETH" ? 58.4m : underlying == "SOL" ? 64.2m : 52.8m;
        decimal roundFactor = underlying == "BTC" ? 1000m : 50m;
        decimal maxPain = Math.Round(spotPrice / roundFactor) * roundFactor;
        if (maxPain > spotPrice) maxPain -= roundFactor;

        var options = new CryptoOptionsMarketDto(
            underlying,
            ImpliedVolatilityDvol: dvol,
            HistoricalVolatility30d: Math.Round(dvol * 0.88m, 1),
            MaxPainStrike: maxPain,
            CurrentSpotPrice: spotPrice,
            PutCallRatioOi: 0.68m,
            PutCallRatioVolume24h: 0.74m,
            Skew25Delta: +1.8m,
            TotalOpenInterestUsd: 18450000000m,
            Total24hVolumeUsd: 1420000000m);

        // 4. Basis & Term Structure Curve
        int daysToQ1 = 65;
        int daysToQ2 = 156;
        decimal q1BasisUsd = Math.Round(spotPrice * 0.0122m, 2);
        decimal q1FuturesPrice = spotPrice + q1BasisUsd;
        decimal q1Yield = daysToQ1 > 0 ? Math.Round((q1BasisUsd / spotPrice) * (365m / daysToQ1) * 100m, 2) : 0m;

        decimal q2BasisUsd = Math.Round(spotPrice * 0.0288m, 2);
        decimal q2FuturesPrice = spotPrice + q2BasisUsd;
        decimal q2Yield = daysToQ2 > 0 ? Math.Round((q2BasisUsd / spotPrice) * (365m / daysToQ2) * 100m, 2) : 0m;

        var contracts = new List<CryptoFuturesContractDto>
        {
            new($"{cleanSymbol} Perpetual", DateTime.UtcNow.AddHours(8), 0, funding.MarkPrice, Math.Round(funding.MarkPrice - spotPrice, 2), Math.Round(((funding.MarkPrice - spotPrice) / spotPrice) * 100m, 3), Math.Round(funding.EstimatedAnnualRate, 2)),
            new($"{underlying} 26-DEC Quarterly", DateTime.UtcNow.AddDays(daysToQ1), daysToQ1, q1FuturesPrice, q1BasisUsd, 1.22m, q1Yield),
            new($"{underlying} 27-MAR Bi-Quarterly", DateTime.UtcNow.AddDays(daysToQ2), daysToQ2, q2FuturesPrice, q2BasisUsd, 2.88m, q2Yield)
        };

        var curve = new CryptoFuturesCurveDto(
            cleanSymbol,
            spotPrice,
            Regime: "Contango",
            AverageAnnualizedBasisPercent: Math.Round((q1Yield + q2Yield) / 2.0m, 2),
            contracts);

        return new CryptoDerivativesHubDto(
            cleanSymbol,
            funding,
            lsDto,
            liqMap,
            options,
            curve,
            DateTime.UtcNow);
    }

    public Task<CryptoOnChainHubDto> GetOnChainHubAsync(CancellationToken ct = default)
    {
        // 1. Exchange Flows (Inflow / Outflow / Reserves)
        var exchangeFlows = new List<CryptoExchangeFlowDto>
        {
            new("Binance", 1240000000m, 1580000000m, -340000000m, 582410m, -0.82m, "Heavy Accumulation / Bullish"),
            new("Coinbase Prime", 890000000m, 1150000000m, -260000000m, 345120m, -1.15m, "Institutional Outflow / Custody"),
            new("OKX", 420000000m, 390000000m, +30000000m, 128450m, +0.25m, "Neutral"),
            new("Bybit", 510000000m, 4850000000m, +25000000m, 64800m, +0.40m, "Perp Collateral Deposit"),
            new("Kraken", 210000000m, 310000000m, -100000000m, 89300m, -0.75m, "Moderate Outflow")
        };
        decimal totalNetExchangeFlow = exchangeFlows.Sum(e => e.NetFlowUsd24h);

        // 2. Whale Transactions Feed (> $5M USD)
        var whaleTxs = new List<CryptoWhaleTransactionDto>
        {
            new("0x4a8b...12c9", "Bitcoin", "BTC", 2450m, 165252500m, "1P5ZEDWT...m9y (Unknown Whale)", "Unknown Whale", "34xp4vRo...rZ6 (Coinbase Prime Custody)", "Coinbase Prime", "Exchange Outflow (Cold Storage Accumulation)", DateTime.UtcNow.AddMinutes(-8)),
            new("0x7e2d...f91a", "Ethereum", "ETH", 45000m, 121297500m, "0x28c6...821a (Bitfinex Hot)", "Bitfinex", "0x53d2...8a39 (Kraken Custody)", "Kraken", "Whale Internal Shuffle / Market Maker Liquidity", DateTime.UtcNow.AddMinutes(-24)),
            new("0x19f0...bc43", "Ethereum", "USDT", 80000000m, 80000000m, "0x5754...119c (Tether Treasury)", "Tether Treasury", "0xdfd5...9a3b (Binance Deposit)", "Binance", "Exchange Inflow (Fresh Bullish Dry Powder)", DateTime.UtcNow.AddMinutes(-42)),
            new("0x9c3e...88ab", "Bitcoin", "BTC", 1200m, 80940000m, "34xp4vRo...rZ6 (Binance Hot)", "Binance", "1LdnfA...8kP (Unknown Cold Wallet)", "Unknown Wallet", "Exchange Outflow (Cold Storage Accumulation)", DateTime.UtcNow.AddHours(-1)),
            new("5yXw...8kLm", "Solana", "SOL", 350000m, 58187500m, "9xQe...4bTq (Whale Staking Account)", "Unknown Whale", "2ojv...8zWk (Binance Hot Wallet)", "Binance", "Exchange Inflow (Sell Pressure)", DateTime.UtcNow.AddHours(-2)),
            new("0x3b1c...d442", "Ethereum", "LINK", 500000m, 5825000m, "0x892a...f71c (Unknown Holder)", "Unknown Holder", "0xbc10...3c99 (Chainlink Staking v0.2)", "Chainlink Pool", "Whale OTC Transfer (Staking Lockup)", DateTime.UtcNow.AddHours(-3))
        };

        // 3. Stablecoin Supply Flows & Mint/Burn
        var stableSupplies = new List<CryptoStablecoinSupplyFlowDto>
        {
            new("USDT (Tether)", 120400000000m, +1000000000m, +3200000000m, 70.8m, "Expanding"),
            new("USDC (Circle)", 35800000000m, +240000000m, +950000000m, 21.0m, "Expanding"),
            new("USDe (Ethena)", 2800000000m, +45000000m, +180000000m, 1.6m, "Rapid Expansion"),
            new("FDUSD (First Digital)", 2400000000m, -20000000m, -80000000m, 1.4m, "Stable"),
            new("PYUSD (PayPal)", 780000000m, +12000000m, +54000000m, 0.5m, "Expanding")
        };

        var mintsBurns = new List<CryptoStablecoinMintBurnDto>
        {
            new("USDT", "MINT", 1000000000m, "Tron (TRC-20)", "Tether Treasury", DateTime.UtcNow.AddHours(-4), "Net Bullish Expansion"),
            new("USDC", "MINT", 250000000m, "Ethereum (ERC-20)", "Circle Treasury", DateTime.UtcNow.AddHours(-7), "Net Bullish Expansion"),
            new("USDT", "MINT", 500000000m, "Ethereum (ERC-20)", "Tether Treasury", DateTime.UtcNow.AddHours(-18), "Net Bullish Expansion"),
            new("FDUSD", "BURN", 25000000m, "BNB Smart Chain (BEP-20)", "Binance Hot", DateTime.UtcNow.AddHours(-22), "Liquidity Contraction")
        };

        // 4. Token Unlocks Calendar
        var unlocks = new List<CryptoTokenUnlockDto>
        {
            new("TIA", "Celestia", DateTime.UtcNow.AddDays(9), 9, 175560000m, 948024000m, 79.9m, "Seed & Core Team", "Extreme Dilution (>7%)"),
            new("SUI", "Sui Network", DateTime.UtcNow.AddDays(12), 12, 64190000m, 136082800m, 2.2m, "Series A/B Investors", "Moderate Risk (1-3%)"),
            new("ARB", "Arbitrum", DateTime.UtcNow.AddDays(16), 16, 92650000m, 52810500m, 2.5m, "Team & Advisors", "Moderate Risk (1-3%)"),
            new("OP", "Optimism", DateTime.UtcNow.AddDays(20), 20, 31340000m, 47636800m, 2.6m, "Core Contributors", "Moderate Risk (1-3%)"),
            new("AVAX", "Avalanche", DateTime.UtcNow.AddDays(24), 24, 1670000m, 47428000m, 0.4m, "Strategic Partners", "Low Risk (<1%)"),
            new("STRK", "Starknet", DateTime.UtcNow.AddDays(28), 28, 64000000m, 24960000m, 3.4m, "Early Investors", "High Dilution (3-7%)"),
            new("WLD", "Worldcoin", DateTime.UtcNow.AddDays(31), 31, 37200000m, 78120000m, 6.8m, "TFH Reserve & Investors", "High Dilution (3-7%)")
        };

        // 5. DEX Liquidity Screening
        var dexPools = new List<CryptoDexPoolDto>
        {
            new("PEPE/WETH", "Uniswap v3", "Ethereum", 0.00000985m, +8.4m, 42500000m, 148200000m, 3.48m, 99.2m, true, true, 18.2m),
            new("WIF/SOL", "Raydium", "Solana", 2.45m, +14.2m, 28400000m, 96500000m, 3.40m, 97.5m, true, true, 14.5m),
            new("AERO/USDC", "Aerodrome", "Base", 1.28m, +5.6m, 85200000m, 62100000m, 0.73m, 98.4m, true, true, 12.0m),
            new("PENDLE/USDT", "Uniswap v3", "Arbitrum", 4.62m, +3.1m, 18400000m, 24800000m, 1.35m, 99.0m, true, true, 9.8m),
            new("BRETT/WETH", "Aerodrome", "Base", 0.088m, +11.8m, 14600000m, 38500000m, 2.63m, 96.8m, true, true, 15.4m),
            new("RAY/SOL", "Raydium", "Solana", 2.88m, +18.4m, 45000000m, 82000000m, 1.82m, 98.1m, true, true, 8.5m)
        };

        // 6. Smart Money Wallets & Copy Alerts
        var smartWallets = new List<CryptoSmartMoneyWalletDto>
        {
            new("0x1111...7e4a", "Wintermute Trading / Algorithmic MM", 4820000m, 78.4m, 82500000m, "ETH, SOL, ARB, ENA", "DEX Arbitrage & Market Making", 3420, true),
            new("0x7104...4b21", "GMX Top Whale / Swing Long", 2650000m, 84.0m, 34200000m, "BTC, ETH, LINK", "Perpetual Momentum Swings", 4890, true),
            new("0x9c82...98f3", "DeFi Alpha Fund / Early Rotation", 1940000m, 72.5m, 19800000m, "PENDLE, AERO, SUI", "Ecosystem Liquidity Farming", 2150, true),
            new("0x3a45...c120", "Memecoin Sniper / Smart Accumulator", 1420000m, 68.2m, 8400000m, "PEPE, WIF, BRETT", "DEX Liquidity Breakouts", 5820, true)
        };

        var smartActivity = new List<CryptoSmartMoneyActivityDto>
        {
            new("0x7a2c...88f1", "GMX Top Whale", "BOUGHT", "ETH", 450m, 1212975m, 2695.50m, "Uniswap v3", DateTime.UtcNow.AddMinutes(-12), "High Conviction Buy"),
            new("0x4e9d...21b0", "Wintermute Trading", "OPENED LONG", "BTC", 25m, 1686250m, 67450.00m, "Hyperliquid", DateTime.UtcNow.AddMinutes(-34), "Accumulation Dip"),
            new("0x8c1b...99a4", "DeFi Alpha Fund", "STAKED", "PENDLE", 120000m, 554400m, 4.62m, "Pendle Finance", DateTime.UtcNow.AddHours(-1), "Long Term Conviction"),
            new("0x2d3e...77c8", "Memecoin Sniper", "BOUGHT", "WIF", 185000m, 453250m, 2.45m, "Raydium", DateTime.UtcNow.AddHours(-2), "Momentum Breakout"),
            new("0x5f6a...11d2", "GMX Top Whale", "SOLD", "SOL", 25000m, 4156250m, 166.25m, "Coinbase Prime", DateTime.UtcNow.AddHours(-4), "Take Profit / Partial Close")
        };

        var hub = new CryptoOnChainHubDto(
            exchangeFlows,
            totalNetExchangeFlow,
            whaleTxs,
            stableSupplies,
            mintsBurns,
            unlocks,
            dexPools,
            smartWallets,
            smartActivity,
            DateTime.UtcNow);

        return Task.FromResult(hub);
    }

    public Task<CryptoExchangeFlowAnalysisHubDto> GetExchangeFlowAnalysisAsync(CancellationToken ct = default)
    {
        var exchanges = new List<CryptoExchangeDetailFlowDto>
        {
            new("Binance", 48000000m, 62000000m, -14000000m, 1240000000m, 1580000000m, -340000000m, -1120000000m, -3450000000m, 582410m, 3950000m, 24500000000m, -0.82m, 2.45m, "Heavy Accumulation / Bullish", "Outflow Accumulation"),
            new("Coinbase Prime", 35000000m, 48000000m, -13000000m, 890000000m, 1150000000m, -260000000m, -840000000m, -2100000000m, 345120m, 2810000m, 7200000000m, -1.15m, 14.80m, "Institutional Outflow / Custody", "Outflow Accumulation"),
            new("OKX", 18000000m, 16000000m, +2000000m, 420000000m, 390000000m, +30000000m, +45000000m, -120000000m, 128450m, 1150000m, 5800000000m, +0.25m, 1.85m, "Neutral Range", "Futures Collateralization"),
            new("Bybit", 24000000m, 22000000m, +2000000m, 510000000m, 485000000m, +25000000m, +80000000m, +190000000m, 64800m, 640000m, 4100000000m, +0.40m, 1.20m, "Perp Margin Deposit", "Futures Collateralization"),
            new("Kraken", 9000000m, 14000000m, -5000000m, 210000000m, 310000000m, -100000000m, -320000000m, -680000000m, 89300m, 980000m, 1900000000m, -0.75m, 3.10m, "Moderate Outflow", "Outflow Accumulation"),
            new("Bitfinex", 5000000m, 7000000m, -2000000m, 115000000m, 145000000m, -30000000m, -95000000m, -210000000m, 192000m, 820000m, 980000000m, -0.42m, 8.40m, "Whale Cold Custody", "Outflow Accumulation"),
            new("Deribit", 8000000m, 6000000m, +2000000m, 180000000m, 160000000m, +20000000m, +60000000m, +140000000m, 42000m, 480000m, 850000000m, +0.65m, 4.20m, "Options Margin Collateral", "Futures Collateralization")
        };

        decimal net1h = exchanges.Sum(e => e.NetFlow1hUsd);
        decimal net24h = exchanges.Sum(e => e.NetFlow24hUsd);
        decimal net7d = exchanges.Sum(e => e.NetFlow7dUsd);
        decimal net30d = exchanges.Sum(e => e.NetFlow30dUsd);

        // Hourly trend over last 24h
        var trend = new List<CryptoExchangeFlowTrendPointDto>();
        decimal cum = 0m;
        var now = DateTime.UtcNow;
        var rng = new Random(42);
        for (int h = 24; h >= 0; h--)
        {
            var t = now.AddHours(-h);
            decimal hourlyFlow = (decimal)(rng.NextDouble() * 35000000 - 48000000); // mostly negative (outflows)
            cum += hourlyFlow;
            decimal price = 67450m + (24 - h) * 45m + (decimal)(rng.NextDouble() * 200 - 100);
            trend.Add(new CryptoExchangeFlowTrendPointDto(t, hourlyFlow, cum, price));
        }

        // Miner-to-Exchange Flows
        var miners = new List<CryptoMinerToExchangeFlowDto>
        {
            new("Foundry USA Pool", 145m, 9780250m, 0.85m, "Normal Operations"),
            new("AntPool", 190m, 12815500m, 0.92m, "Normal Operations"),
            new("F2Pool", 220m, 14839000m, 1.15m, "Light Distribution"),
            new("ViaBTC", 85m, 5733250m, 0.78m, "Normal Operations"),
            new("MaraPool (Marathon)", 40m, 2698000m, 0.60m, "Heavy HODL / Accumulation")
        };

        var hub = new CryptoExchangeFlowAnalysisHubDto(
            exchanges,
            net1h,
            net24h,
            net7d,
            net30d,
            trend,
            miners,
            SpotVsDerivativesRatio: 0.42m,
            MacroFlowVerdict: "Institutional Supply Squeeze: Persistent multi-week spot CEX outflows driven by Coinbase Prime OTC desks and institutional ETF custody withdrawals indicate sustained structural accumulation.",
            DateTime.UtcNow);

        return Task.FromResult(hub);
    }

    public Task<CryptoTokenUnlocksHubDto> GetTokenUnlocksAsync(CancellationToken ct = default)
    {
        var unlocks = new List<CryptoTokenUnlockDetailDto>
        {
            new("TIA", "Celestia", DateTime.UtcNow.AddDays(9), 9, 175560000m, 948024000m, 5.40m, 220000000m, 1000000000m, 79.8m, 17.5m, "Cliff Unlock",
                new List<CryptoUnlockAllocationShareDto>
                {
                    new("Seed & Series A Investors", 88000000m, 475200000m, 50.1m),
                    new("Core Team & Contributors", 52000000m, 280800000m, 29.6m),
                    new("Ecosystem & Foundation Reserve", 35560000m, 192024000m, 20.3m)
                },
                -14.5m, "Delta-Neutral Short Perp Hedge (Basis Lock)", "Extreme Dilution (>7%)"),

            new("SUI", "Sui Network", DateTime.UtcNow.AddDays(12), 12, 64190000m, 136082800m, 2.12m, 2850000000m, 10000000000m, 2.2m, 0.64m, "Cliff + Linear",
                new List<CryptoUnlockAllocationShareDto>
                {
                    new("Series A/B Early VCs", 39000000m, 82680000m, 60.8m),
                    new("Community Reserve", 25190000m, 53402800m, 39.2m)
                },
                -4.2m, "Buy Out-of-Money Puts or Hold Spot", "Moderate Dilution (1-3%)"),

            new("ARB", "Arbitrum", DateTime.UtcNow.AddDays(16), 16, 92650000m, 52810500m, 0.57m, 3600000000m, 10000000000m, 2.5m, 0.93m, "Linear Monthly Cliff",
                new List<CryptoUnlockAllocationShareDto>
                {
                    new("Offchain Labs Team", 56000000m, 31920000m, 60.4m),
                    new("Strategic Investors", 36650000m, 20890500m, 39.6m)
                },
                -3.8m, "Staggered Spot DCA / Minimal Action", "Moderate Dilution (1-3%)"),

            new("OP", "Optimism", DateTime.UtcNow.AddDays(20), 20, 31340000m, 47636800m, 1.52m, 1250000000m, 4294000000m, 2.5m, 0.73m, "Monthly Vesting",
                new List<CryptoUnlockAllocationShareDto>
                {
                    new("Core Contributors", 16500000m, 25080000m, 52.6m),
                    new("Investors", 14840000m, 22556800m, 47.4m)
                },
                -3.1m, "Hedge via Synthetic Futures", "Moderate Dilution (1-3%)"),

            new("AVAX", "Avalanche", DateTime.UtcNow.AddDays(24), 24, 1670000m, 47428000m, 28.40m, 406000000m, 720000000m, 0.4m, 0.23m, "Cliff Unlock",
                new List<CryptoUnlockAllocationShareDto>
                {
                    new("Strategic Partners", 1120000m, 31808000m, 67.1m),
                    new("Ava Labs Foundation", 550000m, 15620000m, 32.9m)
                },
                -0.9m, "Hold Spot / Negligible Impact", "Low Dilution (<1%)"),

            new("STRK", "Starknet", DateTime.UtcNow.AddDays(28), 28, 64000000m, 24960000m, 0.39m, 1900000000m, 10000000000m, 3.4m, 0.64m, "Monthly Linear",
                new List<CryptoUnlockAllocationShareDto>
                {
                    new("Early Backers", 38000000m, 14820000m, 59.4m),
                    new("Core Developers", 26000000m, 10140000m, 40.6m)
                },
                -7.2m, "Short Perp Collar Strategy", "High Dilution (3-7%)"),

            new("WLD", "Worldcoin", DateTime.UtcNow.AddDays(31), 31, 37200000m, 78120000m, 2.10m, 550000000m, 10000000000m, 6.8m, 0.37m, "Daily Vesting Acceleration",
                new List<CryptoUnlockAllocationShareDto>
                {
                    new("TFH Investors", 22000000m, 46200000m, 59.1m),
                    new("Community Operating Fund", 15200000m, 31920000m, 40.9m)
                },
                -8.6m, "Aggressive Spot Reduction", "High Dilution (3-7%)"),

            new("APT", "Aptos", DateTime.UtcNow.AddDays(38), 38, 11310000m, 106314000m, 9.40m, 500000000m, 1100000000m, 2.3m, 1.03m, "Monthly Cliff",
                new List<CryptoUnlockAllocationShareDto>
                {
                    new("Foundation & Community", 6100000m, 57340000m, 53.9m),
                    new("Core Contributors", 5210000m, 48974000m, 46.1m)
                },
                -3.4m, "Neutral Perp Basis", "Moderate Dilution (1-3%)")
        };

        decimal total30d = unlocks.Where(u => u.DaysUntilUnlock <= 30).Sum(u => u.UnlockValueUsd);
        int highRiskCount = unlocks.Count(u => u.DilutionShockRating.Contains("High") || u.DilutionShockRating.Contains("Extreme"));

        var hub = new CryptoTokenUnlocksHubDto(
            unlocks,
            total30d,
            HighestDilutionAssetSymbol: "TIA (79.8% Dilution)",
            highRiskCount,
            DateTime.UtcNow);

        return Task.FromResult(hub);
    }

    public Task<CryptoDexScreenerHubDto> GetDexScreenerAsync(CancellationToken ct = default)
    {
        var pools = new List<CryptoNewDexPoolDto>
        {
            new("PEPE/WETH", "PEPE", "WETH", "Uniswap v3", "Ethereum", 0.00000985m, 12000000m, 42500000m, 148200000m, 3.48m, 4280, 3910, 1.09m, DateTime.UtcNow.AddDays(-540), 777600,
                new CryptoRugpullAuditDto("Pepe", "PEPE", "0x6982508145454ce325ddbe47a25d4ec3d2311933", "Ethereum", 99.2m, "VERIFIED & SAFE", true, false, false, 0m, 0m, false, true, 100m, "Burned to 0x000...dead", 18.2m, 248900, "0xdead...0001", 1, 0)),

            new("WIF/SOL", "WIF", "SOL", "Raydium", "Solana", 2.45m, 8500000m, 28400000m, 96500000m, 3.40m, 6820, 6140, 1.11m, DateTime.UtcNow.AddDays(-320), 460800,
                new CryptoRugpullAuditDto("dogwifhat", "WIF", "EKpQGSJtjMFqKZ9KQanSqYXRcF8fBopzLHYxdM65zcjm", "Solana", 97.5m, "VERIFIED & SAFE", true, false, false, 0m, 0m, false, true, 100m, "Burned LP on Solana", 14.5m, 185400, "9xQe...4bTq", 1, 0)),

            new("AERO/USDC", "AERO", "USDC", "Aerodrome", "Base", 1.28m, 18000000m, 85200000m, 62100000m, 0.73m, 2840, 2410, 1.18m, DateTime.UtcNow.AddDays(-280), 403200,
                new CryptoRugpullAuditDto("Aerodrome Finance", "AERO", "0x940181a94A35A4569E4529A3CDfB74e48FD986ca", "Base", 98.4m, "VERIFIED & SAFE", true, false, false, 0m, 0m, false, true, 95m, "Locked via veAERO protocol", 12.0m, 98200, "0xBase...001", 2, 0)),

            new("PENDLE/USDT", "PENDLE", "USDT", "Uniswap v3", "Arbitrum", 4.62m, 6400000m, 18400000m, 24800000m, 1.35m, 1420, 1280, 1.11m, DateTime.UtcNow.AddDays(-360), 518400,
                new CryptoRugpullAuditDto("Pendle", "PENDLE", "0x0c880f67ed5009c07577ba9e68778d059f487d8c", "Arbitrum", 99.0m, "VERIFIED & SAFE", true, false, false, 0m, 0m, false, true, 100m, "Locked indefinitely", 9.8m, 42100, "0x7a2...11b", 1, 0)),

            new("BRETT/WETH", "BRETT", "WETH", "Aerodrome", "Base", 0.088m, 4500000m, 14600000m, 38500000m, 2.63m, 3890, 3120, 1.25m, DateTime.UtcNow.AddDays(-190), 273600,
                new CryptoRugpullAuditDto("Brett", "BRETT", "0x532f27101965dd16442e59d40670faf5ebb142e4", "Base", 96.8m, "VERIFIED & SAFE", true, false, false, 0m, 0m, false, true, 100m, "Burned to dead address", 15.4m, 114500, "0x4b...99a", 1, 0)),

            new("RAY/SOL", "RAY", "SOL", "Raydium", "Solana", 2.88m, 14000000m, 45000000m, 82000000m, 1.82m, 5120, 4890, 1.05m, DateTime.UtcNow.AddDays(-600), 864000,
                new CryptoRugpullAuditDto("Raydium", "RAY", "4k3Dyjzvzp8eMZWUXbBCjEvwSkkk59S5iCNLY3QrkX6R", "Solana", 98.1m, "VERIFIED & SAFE", true, false, false, 0m, 0m, false, true, 100m, "Burned LP", 8.5m, 134200, "RaydiumCore...01", 1, 0))
        };

        // Newly launched pools in the last 1-6 hours with live Rugpull / Honeypot Audit detection
        var newLaunches = new List<CryptoNewDexPoolDto>
        {
            new("NEO-AI/WETH", "NEO-AI", "WETH", "Uniswap v3", "Ethereum", 0.00042m, 35000m, 84000m, 412000m, 4.90m, 620, 12, 51.6m, DateTime.UtcNow.AddMinutes(-45), 45,
                new CryptoRugpullAuditDto("Neo Artificial Intelligence", "NEO-AI", "0x4f12...99bc", "Ethereum", 12.0m, "HONEYPOT / RUGPULL DETECTED", false, true, true, 2.0m, 99.0m, true, false, 0m, "Unlocked (0 Days)", 78.4m, 410, "0x882a...e71c", 4, 3)),

            new("BASEPUMP/USDC", "BASEPUMP", "USDC", "Aerodrome", "Base", 0.0145m, 120000m, 195000m, 840000m, 4.31m, 1420, 1180, 1.20m, DateTime.UtcNow.AddMinutes(-115), 115,
                new CryptoRugpullAuditDto("Base Pump Protocol", "BASEPUMP", "0x992c...11ab", "Base", 98.0m, "VERIFIED & SAFE", true, false, false, 0m, 0m, false, true, 100m, "Burned LP on Base", 9.2m, 2180, "0x11ab...ff32", 1, 0)),

            new("SOLCAT/SOL", "SOLCAT", "SOL", "Raydium", "Solana", 0.0084m, 45000m, 62000m, 290000m, 4.67m, 890, 410, 2.17m, DateTime.UtcNow.AddMinutes(-180), 180,
                new CryptoRugpullAuditDto("Solana Cat Token", "SOLCAT", "7xPw...99Qm", "Solana", 34.0m, "DANGEROUS / UNLOCKED LP", true, false, false, 0m, 0m, false, false, 12m, "Only 12% locked (88% in dev wallet)", 45.2m, 920, "2mNk...88Lx", 3, 2)),

            new("QUANTUM/WETH", "QUANTUM", "WETH", "Uniswap v3", "Arbitrum", 0.0520m, 80000m, 142000m, 510000m, 3.59m, 940, 810, 1.16m, DateTime.UtcNow.AddMinutes(-270), 270,
                new CryptoRugpullAuditDto("Quantum Arbitrum AI", "QUANTUM", "0x33b1...88ef", "Arbitrum", 95.5m, "VERIFIED & SAFE", true, false, false, 1.5m, 1.5m, false, true, 95m, "Locked 365 Days on UNCX", 11.4m, 1540, "0x77cd...33aa", 1, 0))
        };

        var hub = new CryptoDexScreenerHubDto(
            pools,
            newLaunches,
            TotalPoolsAudited: 148,
            HoneypotsBlockedCount: 32,
            TotalDexVolume24hUsd: 428000000m,
            DateTime.UtcNow);

        return Task.FromResult(hub);
    }
}


