using AzureTest.Domain.Documents.Gym;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class UserWeightLogRepository : DocumentRepository<UserWeightLog>, IUserWeightLogRepository
{
    public UserWeightLogRepository(IMongoDatabase database) : base(database, "UserWeightLogs") { }
}
