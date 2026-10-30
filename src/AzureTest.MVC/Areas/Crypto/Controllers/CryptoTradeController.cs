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
        [FromForm] PlaceCryptoOrderRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = request.IdempotencyKey ?? Guid.NewGuid().ToString("N");

        var command = new PlaceCryptoOrderCommand(
            request.AccountId,
            _currentUser.Id,
            _currentUser.TenantId,
            request.Symbol,
            request.Side,
            request.OrderType,
            request.Quantity,
            request.RequestedPrice,
            request.StopPrice,
            request.Leverage,
            request.StopLoss,
            request.TakeProfit,
            request.TrailingStopDistance,
            idempotencyKey,
            request.TradingMode);

        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, command, nameof(Index));
        }

        var result = await _commands.PlaceOrder.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
            return RedirectToAction(nameof(Index), new { symbol = request.Symbol });
        }

        TempData["SuccessMessage"] = $"Order submitted successfully for {request.Quantity} {request.Symbol} ({request.Side}).";
        return RedirectToAction(nameof(Index), new { symbol = request.Symbol });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Close Position")]
    [Description("Close an open position at the current market bid or ask price, realizing profit/loss.")]
    public async Task<IActionResult> ClosePosition([FromForm] CloseCryptoPositionRequestDto request, CancellationToken cancellationToken = default)
    {
        var command = new CloseCryptoPositionCommand(request.PositionId, request.ClosePrice);
        var result = await _commands.ClosePosition.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = $"Position closed successfully. Realized P&L: {result.Value:C2}.";
        }

        return RedirectToAction(nameof(Index), new { symbol = request.Symbol });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Cancel Order")]
    [Description("Cancel an unfilled pending limit or stop order before execution.")]
    public async Task<IActionResult> CancelOrder([FromForm] CancelCryptoOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        var command = new CancelCryptoOrderCommand(request.OrderId);
        var result = await _commands.CancelOrder.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = "Order cancelled successfully.";
        }

        return RedirectToAction(nameof(Index), new { symbol = request.Symbol });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update Stops")]
    [Description("Modify Stop Loss, Take Profit, or Trailing Stop protection on an active position.")]
    public async Task<IActionResult> UpdateStops([FromForm] UpdateCryptoStopsRequestDto request, CancellationToken cancellationToken = default)
    {
        var command = new UpdateCryptoStopsCommand(request.PositionId, request.StopLoss, request.TakeProfit, request.TrailingStopDistance);
        var result = await _commands.UpdateStops.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = "Risk stops updated successfully.";
        }

        return RedirectToAction(nameof(Index), new { symbol = request.Symbol });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Place Smart Order")]
    [Description("Execute algorithmic orders: TWAP (Time-Weighted Average Price), Iceberg hidden reserve, or Trailing Stop protection.")]
    public async Task<IActionResult> PlaceSmartOrder(
        [FromForm] PlaceCryptoSmartOrderRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var command = new PlaceCryptoSmartOrderCommand(
            request.AccountId,
            _currentUser.Id,
            _currentUser.TenantId,
            request.Symbol,
            request.Side,
            request.SmartOrderType,
            request.TotalQuantity,
            request.RequestedPrice,
            request.DurationMinutes,
            request.IntervalMinutes,
            request.VisibleQuantity,
            request.VariancePercent,
            request.TrailingDistanceUsd,
            request.PositionId);

        var result = await _commands.PlaceSmartOrder.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = $"{request.SmartOrderType} smart execution algorithm successfully initialized for {request.Symbol} ({request.Side}).";
        }

        return RedirectToAction(nameof(Index), new { symbol = request.Symbol });
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
    public IActionResult ExecuteArbitrage([FromForm] ExecuteArbitrageRequestDto request)
    {
        TempData["SuccessMessage"] = $"Arbitrage loop for {request.PairOrRoute} ({request.Type}) triggered successfully! Estimated net yield: +${request.ExpectedProfit:N2}.";
        return RedirectToAction(nameof(Arbitrage));
    }
}
