

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class HabitTrackerRepository : GenericRepository<HabitTracker>, IHabitTrackerRepository
{
    public HabitTrackerRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<HabitTracker>> GetHabitsWithLogsAsync(Guid? userId)
    {
        var query = _dbSet.Include(h => h.Logs).AsQueryable();
        if (userId.HasValue) query = query.Where(h => h.UserId == userId.Value);
        return await query.OrderBy(h => h.CreatedOn).ToListAsync();
    }

    public async Task<HabitTracker?> GetHabitWithLogsByIdAsync(Guid id, Guid? userId)
    {
        var query = _dbSet.Include(h => h.Logs).AsQueryable();
        if (userId.HasValue) query = query.Where(h => h.UserId == userId.Value);
        return await query.FirstOrDefaultAsync(h => h.Id == id);
    }
}
