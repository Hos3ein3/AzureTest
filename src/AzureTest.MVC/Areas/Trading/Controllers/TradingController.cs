using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Commands.CancelTradingOrder;
using AzureTest.Application.Trading.Commands.CloseTradingPosition;
using AzureTest.Application.Trading.Commands.ExecuteTradingJsonBatch;
using AzureTest.Application.Trading.Commands.PlaceTradingOrder;
using AzureTest.Application.Trading.Commands.ResetTradingAccountBalance;
using AzureTest.Application.Trading.Commands.ToggleTradingWatchlist;
using AzureTest.Application.Trading.Queries.GetTradingDashboard;
using AzureTest.Application.Trading.Queries.GetTradingQuotes;
using AzureTest.Application.Trading.Services;
using AzureTest.Contracts.DTOs.Trading;
using AzureTest.Domain.Entities.Trading.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Trading.Controllers;

public sealed class TradingJsonActionRequest
{
    public string Json { get; set; } = string.Empty;
}

[Area("Trading")]
[Authorize]
[DisplayName("Universal Paper Trading")]
[Description("Simplified universal paper trading terminal with multi-asset search, interactive charts, customizable order tickets, and automated risk management.")]
public class TradingController : Controller
{
    private readonly ITradingQueryUnit _queries;
    private readonly ITradingCommandUnit _commands;
    private readonly IValidationService _validator;
    private readonly ICurrentUser _currentUser;
    private readonly ITradingMarketDataService _marketData;

    public TradingController(
        ITradingQueryUnit queries,
        ITradingCommandUnit commands,
        IValidationService validator,
        ICurrentUser currentUser,
        ITradingMarketDataService marketData)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
        _currentUser = currentUser;
        _marketData = marketData;
    }

    [DisplayName("Trading Hub")]
    [Description("Overview paper trading hub displaying watchlist on the left and active portfolio ('What I Have') on the right.")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var result = await _queries.GetDashboardAsync(
            new GetTradingDashboardQuery(_currentUser.Id, _currentUser.TenantId, "BTCUSDT"), ct);

        return result.ToView(this);
    }

    [DisplayName("Trade & Chart Terminal")]
    [Description("Dedicated asset trading terminal featuring the interactive TradingView chart at the top and buy/sell order tickets at the bottom.")]
    public async Task<IActionResult> Trade(string? symbol = "BTCUSDT", CancellationToken ct = default)
    {
        var result = await _queries.GetDashboardAsync(
            new GetTradingDashboardQuery(_currentUser.Id, _currentUser.TenantId, symbol ?? "BTCUSDT"), ct);

        return View(result.Value);
    }

    [HttpGet]
    [DisplayName("Single Asset Live Quote")]
    [Description("Fetch fresh real-time quote for a specific symbol using live feeds for Crypto and Forex.")]
    public async Task<IActionResult> Quote(string symbol, CancellationToken ct = default)
    {
        var quote = await _marketData.GetQuoteAsync(symbol, ct: ct);
        if (quote == null)
            return NotFound(new { error = "Asset quote not found." });

        return Json(quote);
    }

    [HttpPost]
    [DisplayName("Place Order")]
    [Description("Validate and execute a Market, Limit, or Stop paper trade order with multi-level risk parameters.")]
    public async Task<IActionResult> PlaceOrder(
        string symbol,
        string assetName,
        AssetClass assetClass,
        TradingOrderSide side,
        TradingOrderType orderType,
        decimal quantity,
        decimal? price,
        decimal? triggerPrice,
        int leverage,
        decimal? stopLoss,
        decimal? takeProfit,
        string? multipleTakeProfitsJson,
        decimal? trailingStopDistance,
        TimeInForce timeInForce,
        DateTime? expiryDateUtc,
        CancellationToken ct = default)
    {
        var command = new PlaceTradingOrderCommand(
            _currentUser.Id,
            _currentUser.TenantId,
            symbol,
            assetName,
            assetClass,
            side,
            orderType,
            quantity,
            price,
            triggerPrice,
            leverage,
            stopLoss,
            takeProfit,
            multipleTakeProfitsJson,
            trailingStopDistance,
            timeInForce,
            expiryDateUtc);

        var validation = await _validator.ValidateAsync(command, ct);
        if (validation.IsFailure)
        {
            TempData["ErrorMessage"] = validation.Message ?? validation.Error ?? "Order validation failed.";
            return RedirectToAction(nameof(Trade), new { symbol });
        }

        var result = await _commands.PlaceOrderAsync(command, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Order placed successfully for {quantity} {symbol}.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToAction(nameof(Trade), new { symbol });
    }

    [HttpPost]
    [DisplayName("Close Position")]
    [Description("Close an open position at the specified market price and update paper equity and realized PnL.")]
    public async Task<IActionResult> ClosePosition(Guid positionId, decimal? closePrice, string? symbol, string? sourcePage = "index", CancellationToken ct = default)
    {
        var command = new CloseTradingPositionCommand(positionId, _currentUser.Id, _currentUser.TenantId, closePrice);
        var result = await _commands.ClosePositionAsync(command, ct);

        if (result.IsSuccess)
            TempData["SuccessMessage"] = "Position closed successfully.";
        else
            TempData["ErrorMessage"] = result.Error;

        if (string.Equals(sourcePage, "trade", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Trade), new { symbol = symbol ?? "BTCUSDT" });

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [DisplayName("Cancel Order")]
    [Description("Cancel an outstanding pending Limit or Stop order.")]
    public async Task<IActionResult> CancelOrder(Guid orderId, string? symbol, string? sourcePage = "index", CancellationToken ct = default)
    {
        var command = new CancelTradingOrderCommand(orderId, _currentUser.Id, _currentUser.TenantId);
        var result = await _commands.CancelOrderAsync(command, ct);

        if (result.IsSuccess)
            TempData["SuccessMessage"] = "Pending order cancelled.";
        else
            TempData["ErrorMessage"] = result.Error;

        if (string.Equals(sourcePage, "trade", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Trade), new { symbol = symbol ?? "BTCUSDT" });

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [DisplayName("Reset Balance")]
    [Description("Reset paper trading capital balance to the chosen initial allocation amount.")]
    public async Task<IActionResult> ResetBalance(decimal newBalance = 50000m, string? symbol = null, CancellationToken ct = default)
    {
        var command = new ResetTradingAccountBalanceCommand(_currentUser.Id, _currentUser.TenantId, newBalance);
        var result = await _commands.ResetAccountBalanceAsync(command, ct);

        if (result.IsSuccess)
            TempData["SuccessMessage"] = $"Demo account balance successfully reset to ${newBalance:N2}.";
        else
            TempData["ErrorMessage"] = result.Error;

        return RedirectToAction(nameof(Index), new { symbol = symbol ?? "BTCUSDT" });
    }

    [HttpPost]
    [DisplayName("Toggle Watchlist")]
    [Description("Pin or unpin a financial asset to the user's personal trading watchlist.")]
    public async Task<IActionResult> ToggleWatchlist(string symbol, string assetName, AssetClass assetClass, string? returnUrl = null, CancellationToken ct = default)
    {
        var command = new ToggleTradingWatchlistCommand(_currentUser.Id, _currentUser.TenantId, symbol, assetName, assetClass);
        var result = await _commands.ToggleWatchlistAsync(command, ct);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json"))
        {
            return Json(new { success = result.IsSuccess, isWatchlisted = result.Value, symbol });
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index), new { symbol });
    }

    [HttpPost]
    [DisplayName("Execute JSON Batch Actions")]
    [Description("Execute multiple trading actions, programmatic orders, balance resets, or watchlist updates from a raw JSON payload.")]
    public async Task<IActionResult> ExecuteJson([FromBody] TradingJsonActionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request?.Json))
        {
            return Json(new { success = false, message = "JSON payload cannot be empty." });
        }

        var command = new ExecuteTradingJsonBatchCommand(request.Json, _currentUser.Id, _currentUser.TenantId);
        var result = await _commands.ExecuteJsonBatchAsync(command, ct);

        if (result.IsSuccess)
        {
            return Json(new { success = true, results = result.Value });
        }

        return Json(new { success = false, message = result.Error });
    }

    [HttpGet]
    [DisplayName("Query Market Quotes")]
    [Description("Fetch real-time quotes, price changes, and volatility across all supported asset classes.")]
    public async Task<IActionResult> Quotes([FromQuery] AssetClass? assetClass, [FromQuery] string? search, CancellationToken ct = default)
    {
        var result = await _queries.GetQuotesAsync(new GetTradingQuotesQuery(_currentUser.Id, _currentUser.TenantId, assetClass, search), ct);
        return Json(result.Value);
    }
}
