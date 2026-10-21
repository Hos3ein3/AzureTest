using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.CancelCryptoOrder;
using AzureTest.Application.Crypto.Commands.CloseCryptoPosition;
using AzureTest.Application.Crypto.Commands.PlaceCryptoOrder;
using AzureTest.Application.Crypto.Commands.ResetCryptoAccount;
using AzureTest.Application.Crypto.Commands.UpdateCryptoStops;
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
}
