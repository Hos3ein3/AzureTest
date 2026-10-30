using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoAnalysis;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketOverview;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketRegime;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketStructure;
using AzureTest.Application.Crypto.Queries.GetCryptoScreener;
using AzureTest.Application.Crypto.Queries.GetCryptoVolumeProfile;
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
[Description("Technical indicators, ICT/SMC structure, Volume Profile, Anchored VWAP, quant market regimes, and screener.")]
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
    public async Task<IActionResult> Index([FromQuery] CryptoChartRequestDto? request, CancellationToken cancellationToken = default)
    {
        request ??= new CryptoChartRequestDto();
        var result = await _queries.GetAnalysis.HandleAsync(new GetCryptoAnalysisQuery(request.Symbol, request.Timeframe), cancellationToken);
        ViewBag.Symbol = request.Symbol;
        ViewBag.Timeframe = request.Timeframe;
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("ICT / SMC Market Structure")]
    [Description("Institutional Smart Money Concepts: Break of Structure (BOS), Change of Character (CHoCH), Order Blocks (+OB/-OB), Fair Value Gaps (FVG), Liquidity Sweeps, and Premium/Discount OTE zones.")]
    public async Task<IActionResult> MarketStructure([FromQuery] CryptoChartRequestDto? request, CancellationToken cancellationToken = default)
    {
        request ??= new CryptoChartRequestDto();
        var result = await _queries.GetMarketStructure.HandleAsync(new GetCryptoMarketStructureQuery(request.Symbol, request.Timeframe), cancellationToken);
        ViewBag.Symbol = request.Symbol;
        ViewBag.Timeframe = request.Timeframe;
        return result.ToView(this, viewName: "MarketStructure");
    }

    [HttpGet]
    [DisplayName("Volume Profile & Anchored VWAP")]
    [Description("Visible Range Volume Profile (VPVR): Point of Control (POC), Value Area High (VAH), Value Area Low (VAL), High/Low Volume Nodes (HVN/LVN), and Multi-Anchor VWAP with ±1σ and ±2σ standard deviation bands.")]
    public async Task<IActionResult> VolumeProfile([FromQuery] CryptoChartRequestDto? request, CancellationToken cancellationToken = default)
    {
        request ??= new CryptoChartRequestDto();
        var result = await _queries.GetVolumeProfile.HandleAsync(new GetCryptoVolumeProfileQuery(request.Symbol, request.Timeframe), cancellationToken);
        ViewBag.Symbol = request.Symbol;
        ViewBag.Timeframe = request.Timeframe;
        return result.ToView(this, viewName: "VolumeProfile");
    }

    [HttpGet]
    [DisplayName("Market Regime & Quant Analysis")]
    [Description("Quantitative regime detection: ADX trend strength, ATR% volatility, Bollinger Bandwidth squeeze detection, Hurst Exponent mean-reversion analysis, and optimal algorithmic strategy allocation.")]
    public async Task<IActionResult> MarketRegime([FromQuery] CryptoChartRequestDto? request, CancellationToken cancellationToken = default)
    {
        request ??= new CryptoChartRequestDto();
        var result = await _queries.GetMarketRegime.HandleAsync(new GetCryptoMarketRegimeQuery(request.Symbol, request.Timeframe), cancellationToken);
        ViewBag.Symbol = request.Symbol;
        ViewBag.Timeframe = request.Timeframe;
        return result.ToView(this, viewName: "MarketRegime");
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
        [FromQuery] CryptoScreenerFilterDto? filter,
        CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetScreener.HandleAsync(new GetCryptoScreenerQuery(), cancellationToken);
        if (result.IsSuccess)
        {
            var list = System.Linq.Enumerable.AsEnumerable(result.Value);
            if (filter?.MinVolume.HasValue == true) list = System.Linq.Enumerable.Where(list, x => x.Volume24h >= filter.MinVolume.Value);
            if (filter?.MinRsi.HasValue == true) list = System.Linq.Enumerable.Where(list, x => x.Rsi >= filter.MinRsi.Value);
            if (filter?.MaxRsi.HasValue == true) list = System.Linq.Enumerable.Where(list, x => x.Rsi <= filter.MaxRsi.Value);
            if (filter?.MinChange.HasValue == true) list = System.Linq.Enumerable.Where(list, x => x.Change24h >= filter.MinChange.Value);
            if (filter?.MaxChange.HasValue == true) list = System.Linq.Enumerable.Where(list, x => x.Change24h <= filter.MaxChange.Value);
            if (!string.IsNullOrWhiteSpace(filter?.Trend)) list = System.Linq.Enumerable.Where(list, x => x.Recommendation.Contains(filter.Trend, System.StringComparison.OrdinalIgnoreCase));
            result = AzureTest.Application.Abstractions.Result<System.Collections.Generic.IReadOnlyList<CryptoScreenerItemDto>>.Success(System.Linq.Enumerable.ToList(list));
        }

        ViewBag.MinVolume = filter?.MinVolume;
        ViewBag.MinRsi = filter?.MinRsi;
        ViewBag.MaxRsi = filter?.MaxRsi;
        ViewBag.MinChange = filter?.MinChange;
        ViewBag.MaxChange = filter?.MaxChange;
        ViewBag.Trend = filter?.Trend;
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

