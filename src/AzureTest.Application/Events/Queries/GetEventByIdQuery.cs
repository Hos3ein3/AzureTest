using System;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Events;

namespace AzureTest.Application.Events.Queries;

public record GetEventByIdQuery(Guid Id, Guid UserId) : IQuery<EventDto?>;
