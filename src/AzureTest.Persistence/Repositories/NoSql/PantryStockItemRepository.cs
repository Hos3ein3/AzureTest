using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Food;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class PantryStockItemRepository : DocumentRepository<PantryStockItemDocument>, IPantryStockItemRepository
{
    public PantryStockItemRepository(IMongoDatabase database) : base(database, "StockItems") { }
}
