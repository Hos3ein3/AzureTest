using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface ITicketMessageRepository : IGenericRepository<TicketMessage>
{
    Task<IReadOnlyList<TicketMessage>> GetUnreadMessagesAsync(Guid ticketId, Guid userId);
}
