using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoCandles;
using AzureTest.Application.Crypto.Queries.GetCryptoFuturesFunding;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketOverview;
using AzureTest.Application.Crypto.Queries.GetCryptoOrderBook;
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
    [DisplayName("Futures Funding")]
    [Description("Monitor perpetual futures funding rates, annualized yields, open interest, and mark/index prices.")]
    public async Task<IActionResult> Futures(string symbol = "BTCUSDT", CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetFuturesFunding.HandleAsync(new GetCryptoFuturesFundingQuery(symbol), cancellationToken);
        ViewBag.Symbol = symbol;
        return result.ToView(this);
    }
}
