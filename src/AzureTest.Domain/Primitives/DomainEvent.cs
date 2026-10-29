

using System;

namespace AzureTest.Domain.Primitives;

public abstract record DomainEvent(Guid Id, DateTime OccurredOnUtc) : IDomainEvent
{
    public DateTime OccurredOn => OccurredOnUtc;

    protected DomainEvent() : this(Guid.CreateVersion7(), DateTime.UtcNow)
    {
    }
}
