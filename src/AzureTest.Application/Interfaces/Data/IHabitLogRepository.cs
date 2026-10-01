

using System;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface IHabitLogRepository : IGenericRepository<HabitLog>
{
    Task<HabitLog?> GetHabitLogByDateAsync(Guid habitId, DateTime date);
}
