using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Food;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class ShoppingListItemRepository : DocumentRepository<ShoppingListItemDocument>, IShoppingListItemRepository
{
    public ShoppingListItemRepository(IMongoDatabase database) : base(database, "ShoppingListItems") { }
}
