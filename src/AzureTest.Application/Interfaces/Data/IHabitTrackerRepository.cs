using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface IHabitTrackerRepository : IGenericRepository<HabitTracker>
{
    Task<IReadOnlyList<HabitTracker>> GetHabitsWithLogsAsync(Guid? userId, bool includeArchived = false);
    Task<HabitTracker?> GetHabitWithLogsByIdAsync(Guid id, Guid? userId);
}
