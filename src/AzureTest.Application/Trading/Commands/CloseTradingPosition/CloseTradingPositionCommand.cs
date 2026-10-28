using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.Services;
using AzureTest.Domain.Entities.Trading.Enums;
using FluentValidation;

namespace AzureTest.Application.Trading.Commands.CloseTradingPosition;

public sealed record CloseTradingPositionCommand(
    Guid PositionId,
    Guid UserId,
    Guid TenantId,
    decimal? ClosePrice = null) : ICommand<Result<Guid>>;

public sealed class CloseTradingPositionCommandValidator : AbstractValidator<CloseTradingPositionCommand>
{
    public CloseTradingPositionCommandValidator()
    {
        RuleFor(x => x.PositionId).NotEmpty();
    }
}

public sealed class CloseTradingPositionCommandHandler : ICommandHandler<CloseTradingPositionCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITradingMarketDataService _marketData;

    public CloseTradingPositionCommandHandler(IUnitOfWork uow, ITradingMarketDataService marketData)
    {
        _uow = uow;
        _marketData = marketData;
    }

    public async Task<Result<Guid>> HandleAsync(CloseTradingPositionCommand cmd, CancellationToken ct)
    {
        var position = await _uow.TradingPositions.GetByIdAsync(cmd.PositionId);
        if (position == null || position.Status != TradingPositionStatus.Open || position.UserId != cmd.UserId || (cmd.TenantId != Guid.Empty && position.TenantId != cmd.TenantId))
        {
            return Result<Guid>.Failure("Active position not found or already closed.");
        }

        var exitPrice = cmd.ClosePrice ?? await _marketData.GetCurrentPriceAsync(position.Symbol, position.CurrentPrice, ct);
        position.Close(exitPrice);

        var account = await _uow.TradingAccounts.GetByIdAsync(position.AccountId);
        if (account != null)
        {
            account.ApplyRealizedPnl(position.RealizedPnl, position.InitialMargin);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(position.Id);
    }
}
