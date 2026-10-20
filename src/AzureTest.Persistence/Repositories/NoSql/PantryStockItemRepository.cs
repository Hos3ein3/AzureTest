using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Documents.Food;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class PantryStockItemRepository : DocumentRepository<PantryStockItemDocument>, IPantryStockItemRepository
{
    public PantryStockItemRepository(IMongoDatabase database, ITenantProvider? tenantProvider = null) : base(database, "StockItems", tenantProvider) { }
}
