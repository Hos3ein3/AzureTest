using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;

namespace AzureTest.Persistence.Repositories.NoSql;

/// <summary>
/// No-op implementation of IResumeStatusLogRepository used when MongoDB is not configured.
/// All operations complete successfully without doing anything.
/// </summary>
public class NullResumeStatusLogRepository : NullDocumentRepository<ResumeStatusLog>, IResumeStatusLogRepository
{
    public Task<IReadOnlyList<ResumeStatusLog>> GetByResumeIdAsync(Guid resumeId) =>
        Task.FromResult<IReadOnlyList<ResumeStatusLog>>(Array.Empty<ResumeStatusLog>());
}
