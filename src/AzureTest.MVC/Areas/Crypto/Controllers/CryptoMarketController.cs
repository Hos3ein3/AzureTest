using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoCandles;
using AzureTest.Application.Crypto.Queries.GetCryptoDerivativesHub;
using AzureTest.Application.Crypto.Queries.GetCryptoDexScreener;
using AzureTest.Application.Crypto.Queries.GetCryptoExchangeFlow;
using AzureTest.Application.Crypto.Queries.GetCryptoFuturesFunding;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketDataHub;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketOverview;
using AzureTest.Application.Crypto.Queries.GetCryptoOnChainHub;
using AzureTest.Application.Crypto.Queries.GetCryptoOrderBook;
using AzureTest.Application.Crypto.Queries.GetCryptoTokenUnlocks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Crypto.Controllers;

[Area("Crypto")]
[Authorize]
[DisplayName("Crypto Markets")]
[Description("Explore live cryptocurrency prices, market cap rankings, order book depth, kline charts, and futures funding rates.")]
public class CryptoMarketController : Controller
{
    private readonly ICryptoQueryUnit _queries;
    private readonly ICryptoCommandUnit _commands;
    private readonly IValidationService _validator;

    public CryptoMarketController(
        ICryptoQueryUnit queries,
        ICryptoCommandUnit commands,
        IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    [HttpGet]
    [DisplayName("Market Overview")]
    [Description("View live prices, 24h gainers/losers, volume, market cap rankings, and Fear & Greed sentiment.")]
    public async Task<IActionResult> Index([FromQuery] CryptoMarketFilter? filter, CancellationToken cancellationToken)
    {
        filter ??= new CryptoMarketFilter();
        var result = await _queries.GetMarketOverview.HandleAsync(new GetCryptoMarketOverviewQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Candlestick Chart")]
    [Description("Interactive candlestick chart across multiple timeframes (1m to 1d) with volume profile.")]
    public async Task<IActionResult> Chart(string symbol = "BTCUSDT", CryptoTimeframe timeframe = CryptoTimeframe.H1, CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetCandles.HandleAsync(new GetCryptoCandlesQuery(symbol, timeframe, 120), cancellationToken);
        ViewBag.Symbol = symbol;
        ViewBag.Timeframe = timeframe;
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Order Book & Trades")]
    [Description("View real-time order book depth ladder (bids/asks), spreads, and recent market executions.")]
    public async Task<IActionResult> OrderBook(string symbol = "BTCUSDT", CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetOrderBook.HandleAsync(new GetCryptoOrderBookQuery(symbol, 20), cancellationToken);
        ViewBag.Symbol = symbol;
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Derivatives & Futures Hub")]
    [Description("Perpetual funding rates, open interest, long/short ratio, liquidation map & feed, Deribit options IV/Max Pain, and basis curve.")]
    public async Task<IActionResult> Futures(string symbol = "BTCUSDT", CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetDerivativesHub.HandleAsync(new GetCryptoDerivativesHubQuery(symbol), cancellationToken);
        ViewBag.Symbol = symbol;
        return result.ToView(this);
    }


    [HttpGet]
    [DisplayName("Order Flow & Market Data Hub")]
    [Description("Cumulative Volume Delta (CVD), Footprint chart clusters, order book heatmap liquidity walls, Coinbase Premium, and stablecoin depeg radar.")]
    public async Task<IActionResult> OrderFlow(string symbol = "BTCUSDT", CryptoTimeframe timeframe = CryptoTimeframe.H1, CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetMarketDataHub.HandleAsync(new GetCryptoMarketDataHubQuery(symbol, timeframe), cancellationToken);
        ViewBag.Symbol = symbol;
        ViewBag.Timeframe = timeframe;
        return result.ToView(this, viewName: "OrderFlow");
    }

    [HttpGet]
    [DisplayName("On-Chain & Flow Intelligence")]
    [Description("Exchange net inflow/outflow, whale wallet tracking, stablecoin mint/burn flow, token unlocks calendar, DEX screening, and smart money copy alerts.")]
    public async Task<IActionResult> OnChain(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetOnChainHub.HandleAsync(new GetCryptoOnChainHubQuery(), cancellationToken);
        return result.ToView(this, viewName: "OnChain");
    }

    [HttpGet]
    [DisplayName("Exchange Net Flow Analysis")]
    [Description("3.1 Exchange Net Flow Analysis: Inflows, outflows, net reserves across major CEXs, whale deposits, and miner flows.")]
    public async Task<IActionResult> ExchangeFlow(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetExchangeFlow.HandleAsync(new GetCryptoExchangeFlowQuery(), cancellationToken);
        return result.ToView(this, viewName: "ExchangeFlow");
    }

    [HttpGet]
    [DisplayName("Token Unlocks & Dilution Pressure")]
    [Description("3.2 Token Unlocks Calendar & Dilution Pressure: Cliff vs. linear vesting schedules, dilution shock ratings, and hedging strategies.")]
    public async Task<IActionResult> TokenUnlocks(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetTokenUnlocks.HandleAsync(new GetCryptoTokenUnlocksQuery(), cancellationToken);
        return result.ToView(this, viewName: "TokenUnlocks");
    }

    [HttpGet]
    [DisplayName("DEX Liquidity & Rugpull Detector")]
    [Description("3.3 DEX Liquidity & New Pool Rugpull: Automated smart contract audits, honeypot detection, LP lock verification, and capital efficiency.")]
    public async Task<IActionResult> DexScreener(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetDexScreener.HandleAsync(new GetCryptoDexScreenerQuery(), cancellationToken);
        return result.ToView(this, viewName: "DexScreener");
    }
}

