using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Food;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class MealLogRepository : DocumentRepository<MealLogDocument>, IMealLogRepository
{
    public MealLogRepository(IMongoDatabase database) : base(database, "MealLogs") { }
}
