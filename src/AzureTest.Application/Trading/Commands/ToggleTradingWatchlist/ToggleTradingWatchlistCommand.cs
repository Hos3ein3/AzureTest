using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Domain.Entities.Trading.Enums;
using AzureTest.Domain.Exceptions;

using FluentValidation;

namespace AzureTest.Application.Trading.Commands.ToggleTradingWatchlist;

public sealed record ToggleTradingWatchlistCommand(
    Guid UserId,
    Guid TenantId,
    string Symbol,
    string AssetName,
    AssetClass AssetClass) : ICommand<Result<bool>>;

public sealed class ToggleTradingWatchlistCommandValidator : AbstractValidator<ToggleTradingWatchlistCommand>
{
    public ToggleTradingWatchlistCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .MinimumLength(TradingWatchlist.Rules.SymbolMinLength)
            .MaximumLength(TradingWatchlist.Rules.SymbolMaxLength);
    }
}

public sealed class ToggleTradingWatchlistCommandHandler : ICommandHandler<ToggleTradingWatchlistCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;

    public ToggleTradingWatchlistCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<bool>> HandleAsync(ToggleTradingWatchlistCommand cmd, CancellationToken ct)
    {
        var symbol = cmd.Symbol.Trim().ToUpperInvariant();

        var existing = await _uow.TradingWatchlists.GetByUserAndSymbolAsync(cmd.UserId, symbol, ct);

        if (existing is not null)
        {
            _uow.TradingWatchlists.Remove(existing);
            await _uow.SaveChangesAsync(ct);
            return Result<bool>.Success(false);
        }

        _uow.TradingWatchlists.Add(
            TradingWatchlist.Create(cmd.UserId, cmd.TenantId, symbol, cmd.AssetName, cmd.AssetClass));

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            // A parallel request added the same symbol first. The goal state is the same.
        }

        return Result<bool>.Success(true);
    }
}
