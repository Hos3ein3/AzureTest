using AzureTest.Domain.Documents.Gym;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;
namespace AzureTest.Persistence.Repositories.NoSql;
public class UserTrainingSessionRepository : DocumentRepository<UserTrainingSession>, IUserTrainingSessionRepository {
    public UserTrainingSessionRepository(IMongoDatabase database) : base(database, "UserTrainingSessions") { }
}
