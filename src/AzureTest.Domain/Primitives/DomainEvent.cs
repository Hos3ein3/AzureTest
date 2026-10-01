

using System;

namespace AzureTest.Domain.Primitives;

public abstract record DomainEvent(Guid Id, DateTime OccurredOn) : IDomainEvent
{
    protected DomainEvent() : this(Guid.CreateVersion7(), DateTime.UtcNow)
    {
    }
}
