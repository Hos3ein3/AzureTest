using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.PlaceCryptoOrder;

public sealed record PlaceCryptoOrderCommand(
    Guid AccountId,
    Guid UserId,
    Guid TenantId,
    string Symbol,
    CryptoOrderSide Side,
    CryptoOrderType OrderType,
    decimal Quantity,
    decimal RequestedPrice,
    decimal? StopPrice = null,
    int Leverage = 1,
    decimal? StopLoss = null,
    decimal? TakeProfit = null,
    decimal? TrailingStopDistance = null,
    string? IdempotencyKey = null,
    CryptoTradingMode TradingMode = CryptoTradingMode.PaperTrading) : ICommand<Result<Guid>>
{
    public static class Mapping
    {
        public static CryptoOrder ToDomain(PlaceCryptoOrderCommand cmd) =>
            CryptoOrder.Create(
                cmd.AccountId,
                cmd.UserId,
                cmd.TenantId,
                cmd.Symbol,
                cmd.Side,
                cmd.OrderType,
                cmd.Quantity,
                cmd.RequestedPrice,
                cmd.StopPrice,
                cmd.IdempotencyKey,
                null,
                cmd.TradingMode);
    }
}

public sealed class PlaceCryptoOrderCommandValidator : AbstractValidator<PlaceCryptoOrderCommand>
{
    public PlaceCryptoOrderCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .MinimumLength(CryptoOrder.Rules.SymbolMinLength)
            .MaximumLength(CryptoOrder.Rules.SymbolMaxLength);

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(CryptoOrder.Rules.MinQuantity);

        RuleFor(x => x.RequestedPrice)
            .GreaterThanOrEqualTo(CryptoOrder.Rules.MinPrice);

        RuleFor(x => x.Leverage)
            .InclusiveBetween(CryptoPosition.Rules.LeverageMin, CryptoPosition.Rules.LeverageMax);
    }
}

public sealed class PlaceCryptoOrderCommandHandler : ICommandHandler<PlaceCryptoOrderCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICryptoExecutionService _executionService;

    public PlaceCryptoOrderCommandHandler(IUnitOfWork uow, ICryptoExecutionService executionService)
    {
        _uow = uow;
        _executionService = executionService;
    }

    public async Task<Result<Guid>> HandleAsync(PlaceCryptoOrderCommand cmd, CancellationToken ct)
    {
        var account = await _uow.CryptoAccounts.GetByIdAsync(cmd.AccountId);
        if (account == null)
        {
            account = CryptoAccount.Create(cmd.UserId, cmd.TenantId, "Primary Paper Account", "USDT", cmd.TradingMode);
            _uow.CryptoAccounts.Add(account);
            await _uow.SaveChangesAsync(ct);
        }

        var order = PlaceCryptoOrderCommand.Mapping.ToDomain(cmd);

        // Execute via execution service (paper simulation or testnet)
        var executeResult = await _executionService.ExecutePaperOrderAsync(order, account, ct);
        if (executeResult.IsFailure)
        {
            return Result<Guid>.Failure(executeResult.Error);
        }

        var executedOrder = executeResult.Value;

        // If order was filled, open a corresponding position
        if (executedOrder.Status == CryptoOrderStatus.Filled)
        {
            var posResult = await _executionService.OpenPositionAsync(
                executedOrder,
                account,
                cmd.Leverage,
                cmd.StopLoss,
                cmd.TakeProfit,
                cmd.TrailingStopDistance,
                ct);

            if (posResult.IsFailure)
            {
                return Result<Guid>.Failure(posResult.Error);
            }
        }

        return Result<Guid>.Success(executedOrder.Id);
    }
}
