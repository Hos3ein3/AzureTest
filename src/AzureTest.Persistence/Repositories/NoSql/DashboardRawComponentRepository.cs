using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class DashboardRawComponentRepository : DocumentRepository<DashboardRawComponent>, IDashboardRawComponentRepository
{
    public DashboardRawComponentRepository(IMongoDatabase database) : base(database, "DashboardRawComponents")
    {
    }
}
