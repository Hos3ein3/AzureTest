

using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class DashboardComponentRepository : DocumentRepository<DashboardComponent>, IDashboardComponentRepository
{
    public DashboardComponentRepository(IMongoDatabase database) : base(database, "DashboardComponents")
    {
    }
}
