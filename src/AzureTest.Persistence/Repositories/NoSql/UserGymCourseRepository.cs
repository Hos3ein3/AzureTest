using AzureTest.Domain.Documents.Gym;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;
namespace AzureTest.Persistence.Repositories.NoSql;
public class UserGymCourseRepository : DocumentRepository<UserGymCourse>, IUserGymCourseRepository {
    public UserGymCourseRepository(IMongoDatabase database) : base(database, "UserGymCourses") { }
}
