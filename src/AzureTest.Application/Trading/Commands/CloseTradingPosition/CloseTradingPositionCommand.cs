using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
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
    private readonly IEventDispatcher? _eventDispatcher;
    private readonly IAppNotificationService? _notificationService;

    public CloseTradingPositionCommandHandler(
        IUnitOfWork uow,
        ITradingMarketDataService marketData,
        IEventDispatcher? eventDispatcher = null,
        IAppNotificationService? notificationService = null)
    {
        _uow = uow;
        _marketData = marketData;
        _eventDispatcher = eventDispatcher;
        _notificationService = notificationService;
    }

    public CloseTradingPositionCommandHandler(
        IUnitOfWork uow,
        ITradingMarketDataService marketData,
        IAppNotificationService? notificationService)
        : this(uow, marketData, null, notificationService)
    {
    }

    public async Task<Result<Guid>> HandleAsync(CloseTradingPositionCommand cmd, CancellationToken ct)
    {
        var position = await _uow.TradingPositions.GetByIdAsync(cmd.PositionId);
        if (position == null || position.Status != TradingPositionStatus.Open || position.UserId != cmd.UserId || (cmd.TenantId != Guid.Empty && position.TenantId != cmd.TenantId))
        {
            return Result<Guid>.Failure("Active position not found or already closed.");
        }

        decimal exitPrice;
        if (cmd.ClosePrice.HasValue && cmd.ClosePrice.Value > 0)
        {
            exitPrice = cmd.ClosePrice.Value;
        }
        else
        {
            var livePrice = await _marketData.GetCurrentPriceAsync(position.Symbol, 0m, ct);
            if (livePrice > 0)
            {
                exitPrice = livePrice;
            }
            else if (position.CurrentPrice > 0 && position.CurrentPrice != position.EntryPrice)
            {
                exitPrice = position.CurrentPrice;
            }
            else
            {
                var freshQuote = await _marketData.GetQuoteAsync(position.Symbol, null, ct);
                exitPrice = (freshQuote != null && freshQuote.Price > 0)
                    ? freshQuote.Price
                    : (position.CurrentPrice > 0 ? position.CurrentPrice : position.EntryPrice);
            }
        }

        // Close position and raise TradingPositionClosedDomainEvent on AggregateRoot
        position.Close(exitPrice, "Manual");

        // Atomically commit position status and transactional Outbox message
        await _uow.SaveChangesAsync(ct);

        // Dispatch domain events via EventDispatcher to handle balance settlement & side effects
        var domainEvents = _uow.PopDomainEvents();
        if ((domainEvents == null || domainEvents.Count == 0) && position.DomainEvents.Count > 0)
        {
            domainEvents = position.DomainEvents.ToList();
        }

        if (_eventDispatcher != null && domainEvents != null)
        {
            foreach (var domainEvent in domainEvents)
            {
                await _eventDispatcher.PublishAsync(domainEvent, ct);
            }
        }
        else if (domainEvents != null)
        {
            var fallbackHandler = new AzureTest.Application.Trading.EventHandlers.TradingPositionClosedDomainEventHandler(_uow, _notificationService);
            foreach (var domainEvent in domainEvents.OfType<AzureTest.Domain.Entities.Trading.Events.TradingPositionClosedDomainEvent>())
            {
                await fallbackHandler.HandleAsync(domainEvent, ct);
            }
        }

        return Result<Guid>.Success(position.Id);
    }
}
