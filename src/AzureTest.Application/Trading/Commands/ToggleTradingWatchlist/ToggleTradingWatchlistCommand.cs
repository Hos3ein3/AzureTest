using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Domain.Entities.Trading.Enums;
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
        var items = await _uow.TradingWatchlists.GetAllAsync();
        var existing = items.FirstOrDefault(w => w.UserId == cmd.UserId && (cmd.TenantId == Guid.Empty || w.TenantId == cmd.TenantId) && w.Symbol.Equals(cmd.Symbol, StringComparison.OrdinalIgnoreCase));

        bool isNowWatchlisted;
        if (existing != null)
        {
            _uow.TradingWatchlists.Remove(existing);
            isNowWatchlisted = false;
        }
        else
        {
            var newItem = TradingWatchlist.Create(cmd.UserId, cmd.TenantId, cmd.Symbol, cmd.AssetName, cmd.AssetClass);
            _uow.TradingWatchlists.Add(newItem);
            isNowWatchlisted = true;
        }

        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(isNowWatchlisted);
    }
}
