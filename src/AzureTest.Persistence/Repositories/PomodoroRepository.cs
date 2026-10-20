using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.Pomodoro;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class PomodoroRepository : GenericRepository<PomodoroSession>, IPomodoroRepository
{
    public PomodoroRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<PomodoroSession>> GetUserSessionsAsync(
        Guid userId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken ct = default)
    {
        var query = _dbSet.Where(s => s.UserId == userId);

        if (fromUtc.HasValue)
        {
            query = query.Where(s => s.StartedAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(s => s.StartedAtUtc <= toUtc.Value);
        }

        return await query.OrderByDescending(s => s.StartedAtUtc).ToListAsync(ct);
    }

    public async Task<PomodoroSetting?> GetUserSettingAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Set<PomodoroSetting>()
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);
    }

    public async Task AddSettingAsync(PomodoroSetting setting, CancellationToken ct = default)
    {
        await _dbContext.Set<PomodoroSetting>().AddAsync(setting, ct);
    }

    public Task UpdateSettingAsync(PomodoroSetting setting, CancellationToken ct = default)
    {
        _dbContext.Set<PomodoroSetting>().Update(setting);
        return Task.CompletedTask;
    }
}
