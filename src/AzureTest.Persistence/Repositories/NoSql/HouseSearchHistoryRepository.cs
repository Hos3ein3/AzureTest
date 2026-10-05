using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class HouseSearchHistoryRepository : DocumentRepository<HouseSearchHistory>, IHouseSearchHistoryRepository
{
    public HouseSearchHistoryRepository(IMongoDatabase database) : base(database, "HouseSearchHistories")
    {
    }
}
