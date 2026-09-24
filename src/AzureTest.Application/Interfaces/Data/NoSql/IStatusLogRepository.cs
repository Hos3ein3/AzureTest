using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IStatusLogRepository : IDocumentRepository<StatusLog>
{
    Task<IReadOnlyList<StatusLog>> GetByResumeIdAsync(Guid resumeId);
}
