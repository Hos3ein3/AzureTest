using AzureTest.Domain.Documents.Gym;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;
namespace AzureTest.Persistence.Repositories.NoSql;
public class TrainingCourseRepository : DocumentRepository<TrainingCourse>, ITrainingCourseRepository {
    public TrainingCourseRepository(IMongoDatabase database) : base(database, "TrainingCourses") { }
}
