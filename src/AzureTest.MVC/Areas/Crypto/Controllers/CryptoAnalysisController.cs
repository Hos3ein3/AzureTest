using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoAnalysis;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketOverview;
using AzureTest.Application.Crypto.Queries.GetCryptoScreener;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Crypto.Controllers;

[Area("Crypto")]
[Authorize]
[DisplayName("Crypto Analysis")]
[Description("Technical indicators, support/resistance pivots, Fear & Greed sentiment index, on-chain whale activity, and market screener.")]
public class CryptoAnalysisController : Controller
{
    private readonly ICryptoQueryUnit _queries;
    private readonly ICryptoCommandUnit _commands;
    private readonly IValidationService _validator;

    public CryptoAnalysisController(
        ICryptoQueryUnit queries,
        ICryptoCommandUnit commands,
        IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    [HttpGet]
    [DisplayName("Technical Indicators")]
    [Description("Evaluate EMA 9/21/50/200, SMA 20/50, RSI 14, MACD, Bollinger Bands, ATR volatility, and Fibonacci/Classical pivot support and resistance levels.")]
    public async Task<IActionResult> Index(string symbol = "BTCUSDT", CryptoTimeframe timeframe = CryptoTimeframe.H1, CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetAnalysis.HandleAsync(new GetCryptoAnalysisQuery(symbol, timeframe), cancellationToken);
        ViewBag.Symbol = symbol;
        ViewBag.Timeframe = timeframe;
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Sentiment & News")]
    [Description("Alternative.me Fear & Greed Index, social market sentiment gauge, and curated CryptoPanic market-moving headlines.")]
    public async Task<IActionResult> Sentiment(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetMarketOverview.HandleAsync(new GetCryptoMarketOverviewQuery(new CryptoMarketFilter()), cancellationToken);
        return result.ToView(this, viewName: "Sentiment");
    }

    [HttpGet]
    [DisplayName("Market Screener")]
    [Description("Scan and filter coins across the cryptocurrency universe by 24h trading volume, RSI oversold/overbought conditions, price momentum, and trend alignment.")]
    public async Task<IActionResult> Screener(
        [FromQuery] decimal? minVolume,
        [FromQuery] decimal? minRsi,
        [FromQuery] decimal? maxRsi,
        [FromQuery] decimal? minChange,
        [FromQuery] decimal? maxChange,
        [FromQuery] string? trend,
        CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetScreener.HandleAsync(new GetCryptoScreenerQuery(), cancellationToken);
        if (result.IsSuccess)
        {
            var list = System.Linq.Enumerable.AsEnumerable(result.Value);
            if (minVolume.HasValue) list = System.Linq.Enumerable.Where(list, x => x.Volume24h >= minVolume.Value);
            if (minRsi.HasValue) list = System.Linq.Enumerable.Where(list, x => x.Rsi >= minRsi.Value);
            if (maxRsi.HasValue) list = System.Linq.Enumerable.Where(list, x => x.Rsi <= maxRsi.Value);
            if (minChange.HasValue) list = System.Linq.Enumerable.Where(list, x => x.Change24h >= minChange.Value);
            if (maxChange.HasValue) list = System.Linq.Enumerable.Where(list, x => x.Change24h <= maxChange.Value);
            if (!string.IsNullOrWhiteSpace(trend)) list = System.Linq.Enumerable.Where(list, x => x.Recommendation.Contains(trend, System.StringComparison.OrdinalIgnoreCase));
            result = AzureTest.Application.Abstractions.Result<System.Collections.Generic.IReadOnlyList<CryptoScreenerItemDto>>.Success(System.Linq.Enumerable.ToList(list));
        }

        ViewBag.MinVolume = minVolume;
        ViewBag.MinRsi = minRsi;
        ViewBag.MaxRsi = maxRsi;
        ViewBag.MinChange = minChange;
        ViewBag.MaxChange = maxChange;
        ViewBag.Trend = trend;
        return result.ToView(this, viewName: "Screener");
    }

    [HttpGet]
    [DisplayName("On-Chain & Whale Activity")]
    [Description("Monitor large blockchain transfers, whale wallet movements, exchange inflow/outflow metrics, and on-chain accumulation patterns.")]
    public async Task<IActionResult> OnChain(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetMarketOverview.HandleAsync(new GetCryptoMarketOverviewQuery(new CryptoMarketFilter()), cancellationToken);
        return result.ToView(this, viewName: "OnChain");
    }
}
