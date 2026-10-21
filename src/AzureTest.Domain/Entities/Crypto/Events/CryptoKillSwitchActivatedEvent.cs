using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Crypto.Events;

public sealed record CryptoKillSwitchActivatedEvent(Guid AccountId, Guid UserId, string Reason) : DomainEvent;
