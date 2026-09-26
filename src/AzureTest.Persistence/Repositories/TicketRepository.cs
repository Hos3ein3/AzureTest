using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Enums;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
{
    public TicketRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Ticket>> GetAllTicketsAsync(Guid? userId)
    {
        var q = _dbSet.Include(t => t.Messages).AsQueryable();
        if (userId.HasValue)
        {
            q = q.Where(t => t.UserId == userId.Value);
        }
        return await q.OrderByDescending(t => t.CreatedOn).ToListAsync();
    }

    public async Task<Ticket?> GetTicketWithMessagesAsync(Guid id, Guid? userId)
    {
        var q = _dbSet.Include(t => t.Messages).AsQueryable();
        if (userId.HasValue)
        {
            q = q.Where(t => t.UserId == userId.Value);
        }
        return await q.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<Ticket>> GetUnansweredTicketsAsync()
    {
        return await _dbSet
            .Include(t => t.Messages)
            .Where(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress)
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Ticket>> GetUnreadTicketsAsync(Guid userId)
    {
        return await _dbSet
            .Include(t => t.Messages)
            .Where(t => t.UserId == userId && (t.Status == TicketStatus.Open || t.Messages.Any(m => m.UserId != userId && !m.IsReadByCreator)))
            .OrderByDescending(t => t.CreatedOn)
            .ToListAsync();
    }
}
