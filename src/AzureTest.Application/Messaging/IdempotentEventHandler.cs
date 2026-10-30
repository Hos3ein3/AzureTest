using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.Outbox;
using AzureTest.Domain.Exceptions;
using AzureTest.Domain.Primitives;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Messaging;

public abstract class IdempotentEventHandler<TEvent> : IEventHandler<TEvent>
    where TEvent : class, IDomainEvent
{
    protected IUnitOfWork Uow { get; }
    protected ILogger Logger { get; }
    private readonly TimeProvider _time;
    private string HandlerName => GetType().Name;

    protected IdempotentEventHandler(IUnitOfWork uow, ILogger logger, TimeProvider time)
    {
        Uow = uow;
        Logger = logger;
        _time = time;
    }

    public async Task HandleAsync(TEvent @event, CancellationToken ct = default)
    {
        if (await Uow.InboxMessages.HasBeenProcessedAsync(@event.Id, HandlerName, ct))
        {
            Logger.LogInformation("Event {EventId} already processed by {Handler}. Skipping.", @event.Id, HandlerName);
            return;
        }

        // 1. Change state (no save yet)
        await ApplyAsync(@event, ct);

        // 2. Save state + inbox row in ONE transaction
        var now = _time.GetUtcNow().UtcDateTime;
        await Uow.InboxMessages.AddAsync(new InboxMessage
        {
            EventId = @event.Id,
            HandlerName = HandlerName,
            ReceivedOnUtc = now,
            ProcessedOnUtc = now
        }, ct);

        try
        {
            await Uow.SaveChangesAsync(ct);
        }
        catch (DuplicateInboxMessageException)
        {
            Logger.LogInformation("Event {EventId} was processed concurrently by {Handler}. Skipping.", @event.Id, HandlerName);
            return;
        }

        // 3. Side effects only after a successful commit
        try
        {
            await NotifyAsync(@event, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "Notification failed for event {EventId} in {Handler}.", @event.Id, HandlerName);
        }
    }

    protected virtual Task ApplyAsync(TEvent @event, CancellationToken ct) => Task.CompletedTask;
    protected virtual Task NotifyAsync(TEvent @event, CancellationToken ct) => Task.CompletedTask;
}