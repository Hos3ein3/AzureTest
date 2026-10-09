using AzureTest.Domain.Documents.Gym;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;
namespace AzureTest.Persistence.Repositories.NoSql;
public class TrainingRepository : DocumentRepository<Training>, ITrainingRepository {
    public TrainingRepository(IMongoDatabase database) : base(database, "Trainings") { }
}
