using System;
using AzureTest.Domain.Entities.Forex.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Forex.Events;

public sealed record ForexTradeClosedEvent(Guid TradeId, Guid AccountId, string Symbol, decimal NetProfit, ForexExitReason Reason) : DomainEvent;
