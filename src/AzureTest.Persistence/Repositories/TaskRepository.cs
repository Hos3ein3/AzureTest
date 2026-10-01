

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
{
    public TaskRepository(AzureTestDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksForUserAsync(Guid? userId)
    {
        var query = _dbSet.AsQueryable();
        if (userId.HasValue) query = query.Where(t => t.UserId == userId.Value);
        return await query.ToListAsync();
    }
}
