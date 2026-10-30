using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Entities.Outbox;
using AzureTest.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AzureTest.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that converts Domain Events raised on AggregateRoots
/// into OutboxMessage records in the exact same database transaction,
/// guaranteeing at-least-once transactional delivery.
/// </summary>
public sealed class DomainEventsInterceptor : SaveChangesInterceptor
{
    private List<IDomainEvent> _collectedEvents = new();

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ConvertDomainEventsToOutboxMessages(eventData.Context);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ConvertDomainEventsToOutboxMessages(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        return base.SavedChanges(eventData, result);
    }

    /// <summary>
    /// Exposes the collected domain events so Unit of Work or post-save dispatchers can access them.
    /// </summary>
    public IReadOnlyList<IDomainEvent> PopDomainEvents()
    {
        var events = _collectedEvents.AsReadOnly();
        _collectedEvents = new();
        return events;
    }

    private void ConvertDomainEventsToOutboxMessages(DbContext context)
    {
        var aggregateRoots = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        if (aggregateRoots.Count == 0) return;

        foreach (var aggregate in aggregateRoots)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var eventType = domainEvent.GetType();
                var payload = JsonSerializer.Serialize(domainEvent, eventType);

                var outboxMessage = new OutboxMessage
                {
                    //Id = domainEvent.Id != Guid.Empty ? domainEvent.Id : Guid.NewGuid(),
                    EventType = eventType.AssemblyQualifiedName ?? eventType.FullName ?? eventType.Name,
                    Payload = payload,
                    OccurredOnUtc = domainEvent.OccurredOnUtc,
                    RetryCount = 0
                };

                context.Set<OutboxMessage>().Add(outboxMessage);
                _collectedEvents.Add(domainEvent);
            }

            aggregate.ClearDomainEvents();
        }
    }
}
