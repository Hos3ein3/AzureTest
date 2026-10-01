using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class JobSearchHistoryRepository : DocumentRepository<JobSearchHistory>, IJobSearchHistoryRepository
{
    public JobSearchHistoryRepository(IMongoDatabase database) 
        : base(database, "JobSearchHistories")
    {
    }
}
