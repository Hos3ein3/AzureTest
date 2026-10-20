using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Food;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class RecipeRepository : DocumentRepository<RecipeDocument>, IRecipeRepository
{
    public RecipeRepository(IMongoDatabase database) : base(database, "Recipes") { }
}
