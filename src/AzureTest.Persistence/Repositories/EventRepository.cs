

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class EventRepository : GenericRepository<Event>, IEventRepository
{
    public EventRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Event>> GetEventsForUserAsync(Guid? userId)
    {
        var query = _dbSet.AsQueryable();
        if (userId.HasValue) query = query.Where(e => e.UserId == userId.Value);
        return await query.ToListAsync();
    }
}
