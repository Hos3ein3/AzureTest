using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IResumeStatusLogRepository : IDocumentRepository<ResumeStatusLog>
{
    Task<IReadOnlyList<ResumeStatusLog>> GetByResumeIdAsync(Guid resumeId);
}
