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

public sealed class CloseTradingPositionCommandHandler
    : ICommandHandler<CloseTradingPositionCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITradingMarketDataService _marketData;
    private readonly IEventDispatcher _eventDispatcher;

    public CloseTradingPositionCommandHandler(
        IUnitOfWork uow,
        ITradingMarketDataService marketData,
        IEventDispatcher eventDispatcher)
    {
        _uow = uow;
        _marketData = marketData;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<Guid>> HandleAsync(CloseTradingPositionCommand cmd, CancellationToken ct)
    {
        var position = await _uow.TradingPositions.GetByIdAsync(cmd.PositionId);
        if (position is null
            || position.Status != TradingPositionStatus.Open
            || position.UserId != cmd.UserId
            || (cmd.TenantId != Guid.Empty && position.TenantId != cmd.TenantId))
        {
            return Result<Guid>.Failure("Active position not found or already closed.");
        }

        var exitPrice = await ResolveExitPriceAsync(cmd, position, ct);

        position.Close(exitPrice, "Manual");

        await _uow.SaveChangesAsync(ct);

        var domainEvents = _uow.PopDomainEvents();
        if (domainEvents is null || domainEvents.Count == 0)
        {
            domainEvents = position.DomainEvents.ToList();
        }

        foreach (var domainEvent in domainEvents)
        {
            await _eventDispatcher.PublishAsync(domainEvent, ct);
        }

        return Result<Guid>.Success(position.Id);
    }

    private async Task<decimal> ResolveExitPriceAsync(
        CloseTradingPositionCommand cmd,
        AzureTest.Domain.Entities.Trading.TradingPosition position,
        CancellationToken ct)
    {
        if (cmd.ClosePrice is > 0)
            return cmd.ClosePrice.Value;

        var livePrice = await _marketData.GetCurrentPriceAsync(position.Symbol, 0m, ct);
        if (livePrice > 0)
            return livePrice;

        if (position.CurrentPrice > 0 && position.CurrentPrice != position.EntryPrice)
            return position.CurrentPrice;

        var quote = await _marketData.GetQuoteAsync(position.Symbol, null, ct);
        if (quote is { Price: > 0 })
            return quote.Price;

        return position.CurrentPrice > 0 ? position.CurrentPrice : position.EntryPrice;
    }
}