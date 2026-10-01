

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class TicketMessageRepository : GenericRepository<TicketMessage>, ITicketMessageRepository
{
    public TicketMessageRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<TicketMessage>> GetUnreadMessagesAsync(Guid ticketId, Guid userId)
    {
        return await _dbSet
            .Where(x => x.TicketId == ticketId && x.UserId != userId && !x.IsReadByCreator)
            .ToListAsync();
    }
}
