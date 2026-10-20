using System;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;

namespace AzureTest.Persistence.Repositories.NoSql;

/// <summary>
/// No-op implementation of IResumeFilesRepository used when MongoDB is not configured.
/// All operations complete successfully without doing anything.
/// </summary>
public class NullResumeFilesRepository : NullDocumentRepository<ResumeFiles>, IResumeFilesRepository
{
    public Task<ResumeFiles?> GetByResumeIdAsync(Guid resumeId) => Task.FromResult<ResumeFiles?>(null);
}
