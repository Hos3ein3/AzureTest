using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;

namespace AzureTest.Persistence.Repositories.NoSql;

/// <summary>
/// No-op implementation of IStatusLogRepository used when MongoDB is not configured.
/// All operations complete successfully without doing anything.
/// </summary>
public class NullStatusLogRepository : IStatusLogRepository
{
    public Task<StatusLog?> GetByIdAsync(Guid id) => Task.FromResult<StatusLog?>(null);

    public Task<IReadOnlyList<StatusLog>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<StatusLog>>(Array.Empty<StatusLog>());

    public Task<IReadOnlyList<StatusLog>> GetAllAvailableAsync()
        => Task.FromResult<IReadOnlyList<StatusLog>>(Array.Empty<StatusLog>());

    public Task<IReadOnlyList<StatusLog>> FindAsync(Expression<Func<StatusLog, bool>> predicate) =>
        Task.FromResult<IReadOnlyList<StatusLog>>(Array.Empty<StatusLog>());

    public Task AddAsync(StatusLog document) => Task.CompletedTask;

    public Task UpdateAsync(Guid id, StatusLog document) => Task.CompletedTask;

    public Task RemoveAsync(Guid id) => Task.CompletedTask;

    public Task<IReadOnlyList<StatusLog>> GetByResumeIdAsync(Guid resumeId) =>
        Task.FromResult<IReadOnlyList<StatusLog>>(Array.Empty<StatusLog>());
}
