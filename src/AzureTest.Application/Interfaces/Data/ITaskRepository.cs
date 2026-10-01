

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface ITaskRepository : IGenericRepository<TaskItem>
{
    Task<IReadOnlyList<TaskItem>> GetTasksForUserAsync(Guid? userId);
}
