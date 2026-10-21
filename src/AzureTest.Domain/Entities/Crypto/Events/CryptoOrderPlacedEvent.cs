using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Crypto.Events;

public sealed record CryptoOrderPlacedEvent(Guid OrderId, Guid AccountId, string Symbol, decimal Quantity, decimal Price) : DomainEvent;
