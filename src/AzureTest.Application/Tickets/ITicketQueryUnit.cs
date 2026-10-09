using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Application.Tickets.Queries;
using AzureTest.Contracts.DTOs.Tickets;

namespace AzureTest.Application.Tickets;

public interface ITicketQueryUnit
{
    IQueryHandler<GetAllTicketsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<TicketDto, GetAllTicketsFilters>> GetAllTickets { get; }
    IQueryHandler<GetTicketByIdQuery, TicketDto?> GetTicketById { get; }
    IQueryHandler<GetUnansweredTicketsQuery, IReadOnlyList<TicketDto>> GetUnansweredTickets { get; }
    IQueryHandler<GetUnreadTicketsQuery, IReadOnlyList<TicketDto>> GetUnreadTickets { get; }
}
