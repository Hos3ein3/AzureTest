using System;
using System.Collections.Generic;
using System.Linq;
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

    public async Task<HabitLog?> GetHabitLogByDateAsync(Guid habitId, DateOnly localDate)
    {
        return await _dbSet
            .Where(x => x.HabitTrackerId == habitId && x.LocalDate == localDate)
            .OrderByDescending(x => x.LoggedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<HabitLog?> GetHabitLogByDateAsync(Guid habitId, DateTime date)
    {
        var localDate = DateOnly.FromDateTime(date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime());
        return await GetHabitLogByDateAsync(habitId, localDate);
    }

    public async Task<HabitLog?> GetLatestLogAsync(Guid habitId, DateOnly localDate)
    {
        return await _dbSet
            .Where(x => x.HabitTrackerId == habitId && x.LocalDate == localDate)
            .OrderByDescending(x => x.LoggedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<HabitLog>> GetLogsForHabitAsync(Guid habitId, DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        var query = _dbSet.Where(x => x.HabitTrackerId == habitId);
        if (fromDate.HasValue) query = query.Where(x => x.LocalDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(x => x.LocalDate <= toDate.Value);
        return await query.OrderByDescending(x => x.LocalDate).ThenByDescending(x => x.LoggedAt).ToListAsync();
    }
}
