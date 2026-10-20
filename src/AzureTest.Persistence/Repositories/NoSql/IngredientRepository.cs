using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Food;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class IngredientRepository : DocumentRepository<IngredientDocument>, IIngredientRepository
{
    public IngredientRepository(IMongoDatabase database) : base(database, "Ingredients") { }
}
