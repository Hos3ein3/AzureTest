

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface ITicketRepository : IGenericRepository<Ticket>
{
    Task<IReadOnlyList<Ticket>> GetAllTicketsAsync(Guid? userId);
    Task<Ticket?> GetTicketWithMessagesAsync(Guid id, Guid? userId);
    Task<IReadOnlyList<Ticket>> GetUnansweredTicketsAsync();
    Task<IReadOnlyList<Ticket>> GetUnreadTicketsAsync(Guid userId);
}
