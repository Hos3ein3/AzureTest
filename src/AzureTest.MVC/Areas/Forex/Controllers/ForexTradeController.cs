using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.ActivateKillSwitch;
using AzureTest.Application.Forex.Commands.CancelOrder;
using AzureTest.Application.Forex.Commands.CloseTrade;
using AzureTest.Application.Forex.Commands.MoveToBreakEven;
using AzureTest.Application.Forex.Commands.PartialCloseTrade;
using AzureTest.Application.Forex.Commands.PlaceOrder;
using AzureTest.Application.Forex.Commands.ProcessTradingViewWebhook;
using AzureTest.Application.Forex.Commands.ResetDemoAccount;
using AzureTest.Application.Forex.Commands.UpdateTradeStops;
using AzureTest.Application.Forex.Queries.GetForexTradeDashboard;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Areas.Forex.Controllers;

[Area("Forex")]
[Authorize]
[DisplayName("Forex Demo Trading")]
[Description("Demo trading terminal with live order ticket, open positions, pending orders, and margin monitor.")]
public class ForexTradeController : Controller
{
    private readonly IForexQueryUnit _queries;
    private readonly IForexCommandUnit _commands;
    private readonly IValidationService _validator;

    public ForexTradeController(
        IForexQueryUnit queries,
        IForexCommandUnit commands,
        IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    [HttpGet]
    [DisplayName("Trading Terminal")]
    [Description("Access live buy/sell order tickets, active positions, pending orders, and account balance.")]
    public async Task<IActionResult> Index([FromQuery] ForexTradeFilter? filter, CancellationToken cancellationToken)
    {
        filter ??= new ForexTradeFilter();
        var result = await _queries.GetTradeDashboard.HandleAsync(new GetForexTradeDashboardQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return result.ToView(this);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Place Order")]
    [Description("Submit market, limit, or stop order with safe retry idempotency.")]
    public async Task<IActionResult> PlaceOrder(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, command, nameof(Index));
        }

        var result = await _commands.PlaceOrder.HandleAsync(command, cancellationToken);
        return result.ToFormResult(this, command, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Close Position")]
    [Description("Close an open position at the current market bid or ask price.")]
    public async Task<IActionResult> Close(Guid tradeId, CancellationToken cancellationToken)
    {
        var result = await _commands.CloseTrade.HandleAsync(new CloseTradeCommand(tradeId, ForexExitReason.Manual), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Cancel Order")]
    [Description("Cancel an unfilled pending limit or stop order.")]
    public async Task<IActionResult> Cancel(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _commands.CancelOrder.HandleAsync(new CancelOrderCommand(orderId), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update Stops")]
    [Description("Modify Stop Loss, Take Profit, Trailing Stop distance, or lock in Break-Even.")]
    public async Task<IActionResult> UpdateStops(UpdateTradeStopsCommand command, CancellationToken cancellationToken)
    {
        var result = await _commands.UpdateTradeStops.HandleAsync(command, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

using AzureTest.Application.Forex.Commands.ActivateKillSwitch;
using AzureTest.Application.Forex.Commands.MoveToBreakEven;
using AzureTest.Application.Forex.Commands.PartialCloseTrade;
using AzureTest.Application.Forex.Commands.ProcessTradingViewWebhook;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Partial Close Position")]
    [Description("Scale out of an open position by closing a fraction of the lots.")]
    public async Task<IActionResult> PartialClose(Guid tradeId, decimal lotsToClose, CancellationToken cancellationToken)
    {
        var result = await _commands.PartialCloseTrade.HandleAsync(new PartialCloseTradeCommand(tradeId, lotsToClose), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Move to Break-Even")]
    [Description("Lock in profit and remove risk by moving Stop Loss to trade entry price.")]
    public async Task<IActionResult> MoveBreakEven(Guid tradeId, CancellationToken cancellationToken)
    {
        var result = await _commands.MoveToBreakEven.HandleAsync(new MoveToBreakEvenCommand(tradeId), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Emergency Kill Switch")]
    [Description("Instantly liquidate all open positions and cancel all pending orders.")]
    public async Task<IActionResult> KillSwitch(string? reason, CancellationToken cancellationToken)
    {
        var result = await _commands.ActivateKillSwitch.HandleAsync(new ActivateKillSwitchCommand(reason ?? "Emergency Manual Kill Switch Triggered"), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [DisplayName("TradingView Signal Webhook")]
    [Description("Automated signal receiver from TradingView alert webhooks.")]
    public async Task<IActionResult> Webhook([FromBody] TradingViewWebhookPayloadDto payload, CancellationToken cancellationToken)
    {
        if (payload == null) return BadRequest(new { success = false, message = "Empty webhook payload." });
        var result = await _commands.ProcessTradingViewWebhook.HandleAsync(new ProcessTradingViewWebhookCommand(payload), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(new { success = false, error = result.Error });
        }
        return Ok(new { success = true, tradeId = result.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Reset Demo Balance")]
    [Description("Reset paper trading account balance to fresh initial capital.")]
    public async Task<IActionResult> ResetAccount(ResetDemoAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await _commands.ResetDemoAccount.HandleAsync(command, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
