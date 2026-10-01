

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AzureTest.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that collects domain events from AggregateRoots
/// after SaveChanges completes so they can be dispatched.
/// </summary>
public sealed class DomainEventsInterceptor : SaveChangesInterceptor
{
    private List<IDomainEvent> _collectedEvents = new();

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            _collectedEvents = CollectDomainEvents(eventData.Context);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (eventData.Context is not null)
        {
            _collectedEvents = CollectDomainEvents(eventData.Context);
        }

        return base.SavedChanges(eventData, result);
    }

    /// <summary>
    /// Exposes the collected domain events so the Unit of Work can dispatch them.
    /// </summary>
    public IReadOnlyList<IDomainEvent> PopDomainEvents()
    {
        var events = _collectedEvents.AsReadOnly();
        _collectedEvents = new();
        return events;
    }

    private static List<IDomainEvent> CollectDomainEvents(DbContext context)
    {
        var domainEvents = new List<IDomainEvent>();

        var aggregateRoots = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregateRoots)
        {
            domainEvents.AddRange(aggregate.DomainEvents);
            aggregate.ClearDomainEvents();
        }

        return domainEvents;
    }
}
