

using System;

namespace AzureTest.Contracts.Interfaces;

public abstract record IntegrationEvent(Guid Id, DateTime OccurredOn) : IIntegrationEvent
{
    protected IntegrationEvent() : this(Guid.CreateVersion7(), DateTime.UtcNow)
    {
    }
}
