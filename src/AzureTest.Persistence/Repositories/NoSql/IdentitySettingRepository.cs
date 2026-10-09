using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class IdentitySettingRepository : DocumentRepository<IdentitySetting>, IIdentitySettingRepository
{
    public IdentitySettingRepository(IMongoDatabase database) : base(database, "IdentitySettings")
    {
    }
}