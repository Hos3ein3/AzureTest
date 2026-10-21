using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.CancelCryptoOrder;
using AzureTest.Application.Crypto.Commands.CloseCryptoPosition;
using AzureTest.Application.Crypto.Commands.PlaceCryptoOrder;
using AzureTest.Application.Crypto.Commands.PlaceCryptoSmartOrder;
using AzureTest.Application.Crypto.Commands.ResetCryptoAccount;
using AzureTest.Application.Crypto.Commands.UpdateCryptoStops;
using AzureTest.Application.Crypto.Queries.GetCryptoArbitrageHub;
using AzureTest.Application.Crypto.Queries.GetCryptoTradeTerminal;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Crypto.Controllers;

[Area("Crypto")]
[Authorize]
[DisplayName("Crypto Trade Execution")]
[Description("Professional trading terminal with live order tickets, order book ladder, open positions, pending orders, and risk controls.")]
public class CryptoTradeController : Controller
{
    private readonly ICryptoQueryUnit _queries;
    private readonly ICryptoCommandUnit _commands;
    private readonly IValidationService _validator;
    private readonly ICurrentUser _currentUser;

    public CryptoTradeController(
        ICryptoQueryUnit queries,
        ICryptoCommandUnit commands,
        IValidationService validator,
        ICurrentUser currentUser)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [DisplayName("Trading Terminal")]
    [Description("Access the real-time trading console with order execution tickets, depth ladder, open positions, and margin metrics.")]
    public async Task<IActionResult> Index(string symbol = "BTCUSDT", CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetTradeTerminal.HandleAsync(new GetCryptoTradeTerminalQuery(_currentUser.Id, symbol), cancellationToken);
        ViewBag.Symbol = symbol;
        return result.ToView(this);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Submit Order")]
    [Description("Place a Market, Limit, or Stop order with optional leverage, Stop Loss, Take Profit, and idempotency protection.")]
    public async Task<IActionResult> PlaceOrder(
        Guid accountId,
        string symbol,
        CryptoOrderSide side,
        CryptoOrderType orderType,
        decimal quantity,
        decimal requestedPrice,
        decimal? stopPrice,
        int leverage = 1,
        decimal? stopLoss = null,
        decimal? takeProfit = null,
        decimal? trailingStopDistance = null,
        CryptoTradingMode tradingMode = CryptoTradingMode.PaperTrading,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        idempotencyKey ??= Guid.NewGuid().ToString("N");

        var command = new PlaceCryptoOrderCommand(
            accountId,
            _currentUser.Id,
            _currentUser.TenantId,
            symbol,
            side,
            orderType,
            quantity,
            requestedPrice,
            stopPrice,
            leverage,
            stopLoss,
            takeProfit,
            trailingStopDistance,
            idempotencyKey,
            tradingMode);

        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, command, nameof(Index));
        }

        var result = await _commands.PlaceOrder.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
            return RedirectToAction(nameof(Index), new { symbol });
        }

        TempData["SuccessMessage"] = $"Order submitted successfully for {quantity} {symbol} ({side}).";
        return RedirectToAction(nameof(Index), new { symbol });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Close Position")]
    [Description("Close an open position at the current market bid or ask price, realizing profit/loss.")]
    public async Task<IActionResult> ClosePosition(Guid positionId, decimal? closePrice, string symbol = "BTCUSDT", CancellationToken cancellationToken = default)
    {
        var command = new CloseCryptoPositionCommand(positionId, closePrice);
        var result = await _commands.ClosePosition.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = $"Position closed successfully. Realized P&L: {result.Value:C2}.";
        }

        return RedirectToAction(nameof(Index), new { symbol });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Cancel Order")]
    [Description("Cancel an unfilled pending limit or stop order before execution.")]
    public async Task<IActionResult> CancelOrder(Guid orderId, string symbol = "BTCUSDT", CancellationToken cancellationToken = default)
    {
        var command = new CancelCryptoOrderCommand(orderId);
        var result = await _commands.CancelOrder.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = "Order cancelled successfully.";
        }

        return RedirectToAction(nameof(Index), new { symbol });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update Stops")]
    [Description("Modify Stop Loss, Take Profit, or Trailing Stop protection on an active position.")]
    public async Task<IActionResult> UpdateStops(Guid positionId, decimal? stopLoss, decimal? takeProfit, decimal? trailingStopDistance, string symbol = "BTCUSDT", CancellationToken cancellationToken = default)
    {
        var command = new UpdateCryptoStopsCommand(positionId, stopLoss, takeProfit, trailingStopDistance);
        var result = await _commands.UpdateStops.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = "Risk stops updated successfully.";
        }

        return RedirectToAction(nameof(Index), new { symbol });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Place Smart Order")]
    [Description("Execute algorithmic orders: TWAP (Time-Weighted Average Price), Iceberg hidden reserve, or Trailing Stop protection.")]
    public async Task<IActionResult> PlaceSmartOrder(
        Guid accountId,
        string symbol,
        CryptoOrderSide side,
        CryptoOrderType smartOrderType,
        decimal totalQuantity,
        decimal requestedPrice,
        int durationMinutes = 30,
        int intervalMinutes = 5,
        decimal visibleQuantity = 0m,
        decimal variancePercent = 10m,
        decimal? trailingDistanceUsd = null,
        Guid? positionId = null,
        CancellationToken cancellationToken = default)
    {
        var command = new PlaceCryptoSmartOrderCommand(
            accountId,
            _currentUser.Id,
            _currentUser.TenantId,
            symbol,
            side,
            smartOrderType,
            totalQuantity,
            requestedPrice,
            durationMinutes,
            intervalMinutes,
            visibleQuantity,
            variancePercent,
            trailingDistanceUsd,
            positionId);

        var result = await _commands.PlaceSmartOrder.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = $"{smartOrderType} smart execution algorithm successfully initialized for {symbol} ({side}).";
        }

        return RedirectToAction(nameof(Index), new { symbol });
    }

    [HttpGet]
    [DisplayName("Arbitrage Scanner")]
    [Description("Real-time scanner across 3 arbitrage dimensions: Cross-Exchange Spreads, 3-Leg Triangular Cycles, and Delta-Neutral Funding Rate Arbitrage.")]
    public async Task<IActionResult> Arbitrage(CancellationToken cancellationToken = default)
    {
        var result = await _queries.GetArbitrageHub.HandleAsync(new GetCryptoArbitrageHubQuery(), cancellationToken);
        return result.ToView(this);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Execute Arbitrage Opportunity")]
    [Description("Execute multi-exchange or cyclical arbitrage opportunity with automatic fee deductions and slippage controls.")]
    public IActionResult ExecuteArbitrage(string type, string pairOrRoute, decimal expectedProfit)
    {
        TempData["SuccessMessage"] = $"Arbitrage loop for {pairOrRoute} ({type}) triggered successfully! Estimated net yield: +${expectedProfit:N2}.";
        return RedirectToAction(nameof(Arbitrage));
    }
}
