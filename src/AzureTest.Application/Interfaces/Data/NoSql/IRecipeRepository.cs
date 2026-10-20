using AzureTest.Domain.Documents.Food;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IRecipeRepository : IDocumentRepository<RecipeDocument> { }
