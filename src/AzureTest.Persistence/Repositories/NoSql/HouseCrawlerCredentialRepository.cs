using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class HouseCrawlerCredentialRepository : DocumentRepository<HouseCrawlerCredential>, IHouseCrawlerCredentialRepository
{
    public HouseCrawlerCredentialRepository(IMongoDatabase database) : base(database, "HouseCrawlerCredentials")
    {
    }
}
