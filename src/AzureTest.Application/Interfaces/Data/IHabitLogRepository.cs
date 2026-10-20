using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface IHabitLogRepository : IGenericRepository<HabitLog>
{
    Task<HabitLog?> GetHabitLogByDateAsync(Guid habitId, DateOnly localDate);
    Task<HabitLog?> GetHabitLogByDateAsync(Guid habitId, DateTime date);
    Task<HabitLog?> GetLatestLogAsync(Guid habitId, DateOnly localDate);
    Task<IReadOnlyList<HabitLog>> GetLogsForHabitAsync(Guid habitId, DateOnly? fromDate = null, DateOnly? toDate = null);
}
