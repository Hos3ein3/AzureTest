using System;
using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Tickets;

namespace AzureTest.Application.Tickets.Queries;

using AzureTest.Contracts.DTOs.Common;

public record GetAllTicketsQuery(GetAllTicketsFilters? Filters = null, Guid? UserId = null) : IQuery<PagedResult<TicketDto, GetAllTicketsFilters>>
{
    public GetAllTicketsQuery(Guid? userId) : this(null, userId) {}
}
