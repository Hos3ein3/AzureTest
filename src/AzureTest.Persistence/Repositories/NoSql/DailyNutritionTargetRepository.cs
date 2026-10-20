using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Food;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class DailyNutritionTargetRepository : DocumentRepository<DailyNutritionTargetDocument>, IDailyNutritionTargetRepository
{
    public DailyNutritionTargetRepository(IMongoDatabase database) : base(database, "NutritionTargets") { }
}
