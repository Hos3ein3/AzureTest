using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Forex.Events;

public sealed record ForexOrderPlacedEvent(Guid OrderId, Guid AccountId, string Symbol, decimal LotSize) : DomainEvent;
