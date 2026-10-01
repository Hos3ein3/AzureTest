using AzureTest.Domain.Documents;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IJobSearchHistoryRepository : IDocumentRepository<JobSearchHistory>
{
}
