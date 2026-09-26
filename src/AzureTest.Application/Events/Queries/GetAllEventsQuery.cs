using System;
using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Events;

namespace AzureTest.Application.Events.Queries;

public record GetAllEventsQuery(Guid? UserId) : IQuery<IReadOnlyList<EventDto>>;
