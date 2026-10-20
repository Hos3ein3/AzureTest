using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Entities.Pomodoro;

namespace AzureTest.Application.Interfaces.Data;

public interface IPomodoroRepository : IGenericRepository<PomodoroSession>
{
    Task<IReadOnlyList<PomodoroSession>> GetUserSessionsAsync(
        Guid userId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken ct = default);

    Task<PomodoroSetting?> GetUserSettingAsync(Guid userId, CancellationToken ct = default);
    Task AddSettingAsync(PomodoroSetting setting, CancellationToken ct = default);
    Task UpdateSettingAsync(PomodoroSetting setting, CancellationToken ct = default);
}
