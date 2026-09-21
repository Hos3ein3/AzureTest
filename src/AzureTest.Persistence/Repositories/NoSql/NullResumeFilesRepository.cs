using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;

namespace AzureTest.Persistence.Repositories.NoSql;

/// <summary>
/// No-op implementation of IResumeFilesRepository used when MongoDB is not configured.
/// All operations complete successfully without doing anything.
/// </summary>
public class NullResumeFilesRepository : IResumeFilesRepository
{
    public Task<ResumeFiles?> GetByIdAsync(Guid id) => Task.FromResult<ResumeFiles?>(null);

    public Task<IReadOnlyList<ResumeFiles>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<ResumeFiles>>(Array.Empty<ResumeFiles>());

    public Task<IReadOnlyList<ResumeFiles>> FindAsync(Expression<Func<ResumeFiles, bool>> predicate) =>
        Task.FromResult<IReadOnlyList<ResumeFiles>>(Array.Empty<ResumeFiles>());

    public Task AddAsync(ResumeFiles document) => Task.CompletedTask;

    public Task UpdateAsync(Guid id, ResumeFiles document) => Task.CompletedTask;

    public Task RemoveAsync(Guid id) => Task.CompletedTask;

    public Task<ResumeFiles?> GetByResumeIdAsync(Guid resumeId) => Task.FromResult<ResumeFiles?>(null);
}
