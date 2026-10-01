

using System;

namespace AzureTest.Contracts.Interfaces;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
