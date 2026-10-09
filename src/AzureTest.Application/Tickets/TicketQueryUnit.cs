using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Application.Tickets.Queries;
using AzureTest.Contracts.DTOs.Tickets;

namespace AzureTest.Application.Tickets;

public class TicketQueryUnit : ITicketQueryUnit
{
    public IQueryHandler<GetAllTicketsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<TicketDto, GetAllTicketsFilters>> GetAllTickets { get; }
    public IQueryHandler<GetTicketByIdQuery, TicketDto?> GetTicketById { get; }
    public IQueryHandler<GetUnansweredTicketsQuery, IReadOnlyList<TicketDto>> GetUnansweredTickets { get; }
    public IQueryHandler<GetUnreadTicketsQuery, IReadOnlyList<TicketDto>> GetUnreadTickets { get; }

    public TicketQueryUnit(
        IQueryHandler<GetAllTicketsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<TicketDto, GetAllTicketsFilters>> getAllTickets,
        IQueryHandler<GetTicketByIdQuery, TicketDto?> getTicketById,
        IQueryHandler<GetUnansweredTicketsQuery, IReadOnlyList<TicketDto>> getUnansweredTickets,
        IQueryHandler<GetUnreadTicketsQuery, IReadOnlyList<TicketDto>> getUnreadTickets)
    {
        GetAllTickets = getAllTickets;
        GetTicketById = getTicketById;
        GetUnansweredTickets = getUnansweredTickets;
        GetUnreadTickets = getUnreadTickets;
    }
}
