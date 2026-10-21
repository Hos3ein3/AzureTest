using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.PlaceCryptoSmartOrder;

public sealed record PlaceCryptoSmartOrderCommand(
    Guid AccountId,
    Guid UserId,
    Guid TenantId,
    string Symbol,
    CryptoOrderSide Side,
    CryptoOrderType SmartOrderType,
    decimal TotalQuantity,
    decimal TargetPrice,
    int DurationMinutes = 30,
    int IntervalMinutes = 5,
    decimal VisibleQuantity = 0m,
    decimal VariancePercent = 10m,
    decimal? TrailingDistanceUsd = null,
    Guid? PositionId = null) : ICommand<Result<Guid>>;

public sealed class PlaceCryptoSmartOrderCommandValidator : AbstractValidator<PlaceCryptoSmartOrderCommand>
{
    public PlaceCryptoSmartOrderCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(20);

        RuleFor(x => x.TotalQuantity)
            .GreaterThan(0)
            .When(x => x.SmartOrderType != CryptoOrderType.TrailingStop || !x.PositionId.HasValue);

        RuleFor(x => x.SmartOrderType)
            .Must(t => t == CryptoOrderType.Twap || t == CryptoOrderType.Iceberg || t == CryptoOrderType.TrailingStop)
            .WithMessage("Smart order type must be TWAP, Iceberg, or Trailing Stop.");
    }
}

public sealed class PlaceCryptoSmartOrderCommandHandler : ICommandHandler<PlaceCryptoSmartOrderCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICryptoSmartExecutionService _smartExecution;
    private readonly ICryptoExecutionService _execution;

    public PlaceCryptoSmartOrderCommandHandler(
        IUnitOfWork uow,
        ICryptoSmartExecutionService smartExecution,
        ICryptoExecutionService execution)
    {
        _uow = uow;
        _smartExecution = smartExecution;
        _execution = execution;
    }

    public async Task<Result<Guid>> HandleAsync(PlaceCryptoSmartOrderCommand cmd, CancellationToken ct)
    {
        var account = await _uow.CryptoAccounts.GetByIdAsync(cmd.AccountId);
        if (account == null)
        {
            account = CryptoAccount.Create(cmd.UserId, cmd.TenantId, "Primary Paper Account", "USDT", CryptoTradingMode.PaperTrading);
            _uow.CryptoAccounts.Add(account);
            await _uow.SaveChangesAsync(ct);
        }

        if (cmd.SmartOrderType == CryptoOrderType.Twap)
        {
            var twapResult = await _smartExecution.CreateTwapPlanAsync(
                cmd.AccountId,
                cmd.Symbol,
                cmd.Side,
                cmd.TotalQuantity,
                cmd.TargetPrice,
                cmd.DurationMinutes,
                cmd.IntervalMinutes,
                ct);

            if (twapResult.IsFailure)
            {
                return Result<Guid>.Failure(twapResult.Error);
            }

            // Also record parent order in domain
            var order = CryptoOrder.Create(
                cmd.AccountId,
                cmd.UserId,
                cmd.TenantId,
                cmd.Symbol,
                cmd.Side,
                CryptoOrderType.Twap,
                cmd.TotalQuantity,
                cmd.TargetPrice,
                null,
                Guid.NewGuid().ToString("N"),
                twapResult.Value.Id.ToString(),
                account.TradingMode);

            _uow.CryptoOrders.Add(order);
            await _uow.SaveChangesAsync(ct);

            return Result<Guid>.Success(twapResult.Value.Id);
        }
        else if (cmd.SmartOrderType == CryptoOrderType.Iceberg)
        {
            decimal visibleQty = cmd.VisibleQuantity > 0 ? cmd.VisibleQuantity : (cmd.TotalQuantity * 0.20m);
            var icebergResult = await _smartExecution.CreateIcebergPlanAsync(
                cmd.AccountId,
                cmd.Symbol,
                cmd.Side,
                cmd.TotalQuantity,
                visibleQty,
                cmd.TargetPrice,
                cmd.VariancePercent,
                ct);

            if (icebergResult.IsFailure)
            {
                return Result<Guid>.Failure(icebergResult.Error);
            }

            var order = CryptoOrder.Create(
                cmd.AccountId,
                cmd.UserId,
                cmd.TenantId,
                cmd.Symbol,
                cmd.Side,
                CryptoOrderType.Iceberg,
                cmd.TotalQuantity,
                cmd.TargetPrice,
                null,
                Guid.NewGuid().ToString("N"),
                icebergResult.Value.Id.ToString(),
                account.TradingMode);

            _uow.CryptoOrders.Add(order);
            await _uow.SaveChangesAsync(ct);

            return Result<Guid>.Success(icebergResult.Value.Id);
        }
        else if (cmd.SmartOrderType == CryptoOrderType.TrailingStop)
        {
            if (cmd.PositionId.HasValue)
            {
                var trailResult = await _execution.UpdateStopsAsync(
                    cmd.PositionId.Value,
                    null,
                    null,
                    cmd.TrailingDistanceUsd ?? 500m,
                    ct);

                if (trailResult.IsFailure)
                {
                    return Result<Guid>.Failure(trailResult.Error);
                }

                return Result<Guid>.Success(cmd.PositionId.Value);
            }
            else
            {
                // New trailing stop order
                var order = CryptoOrder.Create(
                    cmd.AccountId,
                    cmd.UserId,
                    cmd.TenantId,
                    cmd.Symbol,
                    cmd.Side,
                    CryptoOrderType.TrailingStop,
                    cmd.TotalQuantity,
                    cmd.TargetPrice,
                    cmd.TargetPrice - (cmd.TrailingDistanceUsd ?? 500m),
                    Guid.NewGuid().ToString("N"),
                    Guid.NewGuid().ToString("N"),
                    account.TradingMode);

                _uow.CryptoOrders.Add(order);
                await _uow.SaveChangesAsync(ct);

                return Result<Guid>.Success(order.Id);
            }
        }

        return Result<Guid>.Failure("Unsupported smart order algorithm.");
    }
}
