using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;

namespace AzureTest.Persistence.Repositories.NoSql;

/// <summary>
/// No-op implementation of IResumeStatusLogRepository used when MongoDB is not configured.
/// All operations complete successfully without doing anything.
/// </summary>
public class NullResumeStatusLogRepository : IResumeStatusLogRepository
{
    public Task<ResumeStatusLog?> GetByIdAsync(Guid id,CancellationToken cancellationToken = default) => Task.FromResult<ResumeStatusLog?>(null);

    public Task<IReadOnlyList<ResumeStatusLog>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ResumeStatusLog>>(Array.Empty<ResumeStatusLog>());

    public Task<IReadOnlyList<ResumeStatusLog>> GetAllAvailableAsync()
        => Task.FromResult<IReadOnlyList<ResumeStatusLog>>(Array.Empty<ResumeStatusLog>());

    public Task<IReadOnlyList<ResumeStatusLog>> FindAsync(Expression<Func<ResumeStatusLog, bool>> predicate) =>
        Task.FromResult<IReadOnlyList<ResumeStatusLog>>(Array.Empty<ResumeStatusLog>());

    public Task AddAsync(ResumeStatusLog document,CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task UpdateAsync(Guid id, ResumeStatusLog document,CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RemoveAsync(Guid id,CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<ResumeStatusLog>> GetByResumeIdAsync(Guid resumeId) =>
        Task.FromResult<IReadOnlyList<ResumeStatusLog>>(Array.Empty<ResumeStatusLog>());
}
