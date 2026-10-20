using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Food;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class StockMovementRepository : DocumentRepository<StockMovementDocument>, IStockMovementRepository
{
    public StockMovementRepository(IMongoDatabase database) : base(database, "StockMovements") { }
}
