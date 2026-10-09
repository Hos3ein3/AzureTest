using System;
using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Events;

namespace AzureTest.Application.Events.Queries;

using AzureTest.Contracts.DTOs.Common;

public record GetAllEventsQuery(GetAllEventsFilters? Filters = null, Guid? UserId = null) : IQuery<PagedResult<EventDto, GetAllEventsFilters>>
{
    public GetAllEventsQuery(Guid? userId) : this(null, userId) {}
}
