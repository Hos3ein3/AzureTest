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
using AzureTest.Application.Trading.Commands.ModifyTradingPosition;
using AzureTest.Application.Trading.Commands.PlaceTradingOrder;
using AzureTest.Application.Trading.Commands.ResetTradingAccountBalance;
using AzureTest.Application.Trading.Commands.SyncTradingPrices;
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

    [DisplayName("Quantitative Performance & Pro Analytics")]
    [Description("Deep-dive trader telemetry, cumulative equity trajectory, Monte Carlo forward projections, day-of-week PnL, trader competency radar, and risk diagnostics.")]
    public async Task<IActionResult> Analytics(CancellationToken ct = default)
    {
        var result = await _queries.GetDashboardAsync(
            new GetTradingDashboardQuery(_currentUser.Id, _currentUser.TenantId, "BTCUSDT"), ct);

        return View(result.Value);
    }

    [DisplayName("Trade History & Receipts")]
    [Description("Comprehensive trade history ledger showing closed positions, executed prices, margin used, PnL, leverage, and full receipts.")]
    public async Task<IActionResult> History([FromQuery] TradingHistoryFilterDto? filter, CancellationToken ct = default)
    {
        var result = await _queries.GetHistoryAsync(
            new AzureTest.Application.Trading.Queries.GetTradingHistory.GetTradingHistoryQuery(
                _currentUser.Id,
                _currentUser.TenantId,
                filter?.Search,
                filter?.Side,
                filter?.AssetClass,
                filter?.Outcome), ct);

        return View(result.Value);
    }

    [HttpGet]
    [AllowAnonymous]
    [DisplayName("Single Asset Live Quote")]
    [Description("Fetch fresh real-time quote for a specific symbol using live feeds for Crypto and Forex.")]
    public async Task<IActionResult> Quote(string symbol, CancellationToken ct = default)
    {
        var quote = await _marketData.GetQuoteAsync(symbol, ct: ct);
        if (quote == null)
            return NotFound(new { error = "Asset quote not found." });

        if (User.Identity?.IsAuthenticated == true)
        {
            try
            {
                await _commands.SyncPricesAsync(new SyncTradingPricesCommand(), ct);
            }
            catch
            {
                // Background price sync best-effort during quote polling
            }
        }

        return Json(quote);
    }

    [HttpGet]
    [AllowAnonymous]
    [DisplayName("Live Positions and Account Status")]
    [Description("Fetch active open positions, closed positions, pending orders, and account balance for live auto-updating terminal tables.")]
    public async Task<IActionResult> LiveStatus(string? symbol = "BTCUSDT", CancellationToken ct = default)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            try
            {
                await _commands.SyncPricesAsync(new SyncTradingPricesCommand(), ct);
            }
            catch
            {
                // Background price sync best-effort during live status polling
            }
        }

        var result = await _queries.GetDashboardAsync(
            new GetTradingDashboardQuery(_currentUser.Id, _currentUser.TenantId, symbol ?? "BTCUSDT"), ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var dto = result.Value;
        return Json(new {
            success = true,
            account = dto.Account,
            openPositions = dto.OpenPositions,
            closedPositions = dto.ClosedPositions,
            pendingOrders = dto.PendingOrders,
            selectedQuote = dto.SelectedQuote
        });
    }

    [HttpGet]
    [AllowAnonymous]
    [DisplayName("Search Trading Assets")]
    [Description("Search instruments dynamically across built-in catalog, Binance live pairs, and DexScreener DEX tokens.")]
    public async Task<IActionResult> Search(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Json(Array.Empty<TradingQuoteDto>());

        var quotes = await _marketData.SearchAssetsAsync(query.Trim(), ct: ct);
        return Json(quotes);
    }

    [HttpPost]
    [DisplayName("Place Order")]
    [Description("Validate and execute a Market, Limit, or Stop paper trade order with multi-level risk parameters.")]
    public async Task<IActionResult> PlaceOrder([FromForm] PlaceTradingOrderRequestDto request, CancellationToken ct = default)
    {
        decimal? ParseDecimal(string key, decimal? boundValue)
        {
            if (Request.HasFormContentType && Request.Form.TryGetValue(key, out var rawVal) && !string.IsNullOrWhiteSpace(rawVal))
            {
                var s = rawVal.ToString().Trim().Replace(',', '.');
                if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                {
                    return parsed;
                }
            }
            return boundValue;
        }

        var quantity = ParseDecimal("quantity", request.Quantity) ?? request.Quantity;
        var price = ParseDecimal("price", request.Price);
        var triggerPrice = ParseDecimal("triggerPrice", request.TriggerPrice);
        var stopLoss = ParseDecimal("stopLoss", request.StopLoss);
        var takeProfit = ParseDecimal("takeProfit", request.TakeProfit);
        var trailingStopDistance = ParseDecimal("trailingStopDistance", request.TrailingStopDistance);

        var command = new PlaceTradingOrderCommand(
            _currentUser.Id,
            _currentUser.TenantId,
            request.Symbol,
            request.AssetName,
            request.AssetClass,
            request.Side,
            request.OrderType,
            quantity,
            price,
            triggerPrice,
            request.Leverage,
            stopLoss,
            takeProfit,
            request.MultipleTakeProfitsJson,
            trailingStopDistance,
            request.TimeInForce,
            request.ExpiryDateUtc);

        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json");

        var validation = await _validator.ValidateAsync(command, ct);
        if (validation.IsFailure)
        {
            var err = validation.Message ?? validation.Error ?? "Order validation failed.";
            if (isAjax) return Json(new { success = false, error = err });
            TempData["ErrorMessage"] = err;
            return RedirectToAction(nameof(Trade), new { symbol = request.Symbol });
        }

        var result = await _commands.PlaceOrderAsync(command, ct);
        if (result.IsSuccess)
        {
            var msg = $"Order placed successfully for {quantity:N4} {request.Symbol}.";
            if (isAjax) return Json(new { success = true, message = msg, orderId = result.Value });
            TempData["SuccessMessage"] = msg;
        }
        else
        {
            if (isAjax) return Json(new { success = false, error = result.Error });
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToAction(nameof(Trade), new { symbol = request.Symbol });
    }

    [HttpPost]
    [DisplayName("Close Position")]
    [Description("Close an open position at the specified market price and update paper equity and realized PnL.")]
    public async Task<IActionResult> ClosePosition([FromForm] CloseTradingPositionRequestDto request, CancellationToken ct = default)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json");

        var closePrice = request.ClosePrice;
        if (Request.HasFormContentType && Request.Form.TryGetValue("closePrice", out var cpVal) && !string.IsNullOrWhiteSpace(cpVal))
        {
            var s = cpVal.ToString().Trim().Replace(',', '.');
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedCp) && parsedCp > 0)
            {
                closePrice = parsedCp;
            }
        }

        var command = new CloseTradingPositionCommand(request.PositionId, _currentUser.Id, _currentUser.TenantId, closePrice);
        var result = await _commands.ClosePositionAsync(command, ct);

        if (result.IsSuccess)
        {
            var msg = "Position closed successfully.";
            var dashRes = await _queries.GetDashboardAsync(new GetTradingDashboardQuery(_currentUser.Id, _currentUser.TenantId, request.Symbol ?? "BTCUSDT"), ct);
            if (dashRes.IsSuccess)
            {
                var closed = dashRes.Value.ClosedPositions.FirstOrDefault(c => c.Id == request.PositionId);
                if (closed != null)
                {
                    var pnlFormatted = AzureTest.Application.Trading.Services.TradingNotificationFormatter.FormatPnl(closed.RealizedPnl);
                    msg = $"Position closed successfully. Realized PnL: {pnlFormatted}.";
                }
            }

            if (isAjax) return Json(new { success = true, message = msg });
            TempData["SuccessMessage"] = msg;
        }
        else
        {
            if (isAjax) return Json(new { success = false, error = result.Error });
            TempData["ErrorMessage"] = result.Error;
        }

        if (string.Equals(request.SourcePage, "trade", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Trade), new { symbol = request.Symbol ?? "BTCUSDT" });

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [AllowAnonymous]
    [DisplayName("Modify Position Stops")]
    [Description("Update Stop Loss and Take Profit levels on an active open position.")]
    public async Task<IActionResult> ModifyPosition([FromForm] ModifyTradingPositionRequestDto request, CancellationToken ct = default)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json");

        if (User.Identity?.IsAuthenticated != true || _currentUser.Id == Guid.Empty)
        {
            if (isAjax) return Json(new { success = false, error = "Authentication required." });
            return Challenge();
        }

        var stopLoss = request.StopLoss;
        var takeProfit = request.TakeProfit;
        var trailingStopDistance = request.TrailingStopDistance;
        var multipleTakeProfitsJson = request.MultipleTakeProfitsJson;

        if (Request.HasFormContentType)
        {
            if (Request.Form.TryGetValue("stopLoss", out var slVal))
            {
                var s = slVal.ToString().Trim().Replace(',', '.');
                if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedSl))
                    stopLoss = parsedSl > 0 ? parsedSl : null;
                else if (string.IsNullOrWhiteSpace(s))
                    stopLoss = null;
            }
            if (Request.Form.TryGetValue("takeProfit", out var tpVal))
            {
                var s = tpVal.ToString().Trim().Replace(',', '.');
                if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedTp))
                    takeProfit = parsedTp > 0 ? parsedTp : null;
                else if (string.IsNullOrWhiteSpace(s))
                    takeProfit = null;
            }
            if (Request.Form.TryGetValue("trailingStopDistance", out var trailVal) && !string.IsNullOrWhiteSpace(trailVal))
            {
                var s = trailVal.ToString().Trim().Replace(',', '.');
                if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedTrail))
                    trailingStopDistance = parsedTrail > 0 ? parsedTrail : null;
                else if (string.IsNullOrWhiteSpace(s))
                    trailingStopDistance = null;
            }
            if (Request.Form.TryGetValue("multipleTakeProfitsJson", out var multiVal))
            {
                var s = multiVal.ToString().Trim();
                multipleTakeProfitsJson = string.IsNullOrWhiteSpace(s) ? null : s;
            }
        }

        var command = new ModifyTradingPositionCommand(request.PositionId, _currentUser.Id, _currentUser.TenantId, stopLoss, takeProfit, trailingStopDistance, multipleTakeProfitsJson);
        var result = await _commands.ModifyPositionAsync(command, ct);

        if (result.IsSuccess)
        {
            if (isAjax) return Json(new { success = true, message = "Position protection stops updated successfully." });
            TempData["SuccessMessage"] = "Position protection stops updated successfully.";
        }
        else
        {
            if (isAjax) return Json(new { success = false, error = result.Error });
            TempData["ErrorMessage"] = result.Error;
        }

        if (string.Equals(request.SourcePage, "trade", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Trade), new { symbol = request.Symbol ?? "BTCUSDT" });

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [DisplayName("Cancel Order")]
    [Description("Cancel an outstanding pending Limit or Stop order.")]
    public async Task<IActionResult> CancelOrder([FromForm] CancelTradingOrderRequestDto request, CancellationToken ct = default)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json");

        var command = new CancelTradingOrderCommand(request.OrderId, _currentUser.Id, _currentUser.TenantId);
        var result = await _commands.CancelOrderAsync(command, ct);

        if (result.IsSuccess)
        {
            if (isAjax) return Json(new { success = true, message = "Pending order cancelled." });
            TempData["SuccessMessage"] = "Pending order cancelled.";
        }
        else
        {
            if (isAjax) return Json(new { success = false, error = result.Error });
            TempData["ErrorMessage"] = result.Error;
        }

        if (string.Equals(request.SourcePage, "trade", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Trade), new { symbol = request.Symbol ?? "BTCUSDT" });

        return RedirectToAction(nameof(Index));
    }



    [HttpPost]
    [DisplayName("Reset Balance")]
    [Description("Reset paper trading capital balance to the chosen initial allocation amount.")]
    public async Task<IActionResult> ResetBalance([FromForm] ResetTradingBalanceRequestDto request, CancellationToken ct = default)
    {
        var command = new ResetTradingAccountBalanceCommand(_currentUser.Id, _currentUser.TenantId, request.NewBalance);
        var result = await _commands.ResetAccountBalanceAsync(command, ct);

        if (result.IsSuccess)
            TempData["SuccessMessage"] = $"Demo account balance successfully reset to ${request.NewBalance:N2}.";
        else
            TempData["ErrorMessage"] = result.Error;

        return RedirectToAction(nameof(Index), new { symbol = request.Symbol ?? "BTCUSDT" });
    }

    [HttpPost]
    [DisplayName("Toggle Watchlist")]
    [Description("Pin or unpin a financial asset to the user's personal trading watchlist.")]
    public async Task<IActionResult> ToggleWatchlist([FromForm] ToggleTradingWatchlistRequestDto request, CancellationToken ct = default)
    {
        var command = new ToggleTradingWatchlistCommand(_currentUser.Id, _currentUser.TenantId, request.Symbol, request.AssetName, request.AssetClass);
        var result = await _commands.ToggleWatchlistAsync(command, ct);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json"))
        {
            return Json(new { success = result.IsSuccess, isWatchlisted = result.Value, symbol = request.Symbol });
        }

        if (!string.IsNullOrEmpty(request.ReturnUrl) && Url.IsLocalUrl(request.ReturnUrl))
            return Redirect(request.ReturnUrl);

        return RedirectToAction(nameof(Index), new { symbol = request.Symbol });
    }

    [HttpPost]
    [DisplayName("Execute JSON Batch Actions")]
    [Description("Execute multiple trading actions, programmatic orders, balance resets, or watchlist updates from a raw JSON payload.")]
    public async Task<IActionResult> ExecuteJson([FromBody] TradingJsonActionRequestDto request, CancellationToken ct = default)
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
    [AllowAnonymous]
    [DisplayName("Query Market Quotes")]
    [Description("Fetch real-time quotes, price changes, and volatility across all supported asset classes.")]
    public async Task<IActionResult> Quotes([FromQuery] AssetClass? assetClass, [FromQuery] string? search, CancellationToken ct = default)
    {
        var result = await _queries.GetQuotesAsync(new GetTradingQuotesQuery(_currentUser.Id, _currentUser.TenantId, assetClass, search), ct);
        return Json(result.Value);
    }
}
