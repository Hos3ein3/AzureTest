using System;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class HabitLogRepository : GenericRepository<HabitLog>, IHabitLogRepository
{
    public HabitLogRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<HabitLog?> GetHabitLogByDateAsync(Guid habitId, DateTime date)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.HabitTrackerId == habitId && x.LogDate.Date == date.Date);
    }
}
