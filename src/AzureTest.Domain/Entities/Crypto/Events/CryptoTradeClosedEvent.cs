using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Crypto.Events;

public sealed record CryptoTradeClosedEvent(Guid PositionId, Guid AccountId, string Symbol, decimal RealizedPnl, decimal ReturnPercentage) : DomainEvent;
