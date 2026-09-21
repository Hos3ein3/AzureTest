using System;
using System.Threading.Tasks;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IResumeFilesRepository : IDocumentRepository<ResumeFiles>
{
    // Fetch by SQL entity's ResumeId
    Task<ResumeFiles?> GetByResumeIdAsync(Guid resumeId);
}
