import os

def write_file(path, content):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w") as f:
        f.write(content.strip() + "\n")

# 1. Domain Enums
write_file("src/AzureTest.Domain/Enums/TrainingType.cs", """
namespace AzureTest.Domain.Enums;
public enum TrainingType { DurationAndRest = 1, SetAndRest = 2 }
""")

# 2. Domain Documents
write_file("src/AzureTest.Domain/Documents/Gym/Training.cs", """
using System;
using AzureTest.Domain.Enums;
namespace AzureTest.Domain.Documents.Gym;
public class Training {
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public TrainingType Type { get; set; } = TrainingType.SetAndRest;
    public double BaseCaloriesBurn { get; set; }
}
""")

write_file("src/AzureTest.Domain/Documents/Gym/TrainingCourse.cs", """
using System;
using System.Collections.Generic;
namespace AzureTest.Domain.Documents.Gym;
public class TrainingCourse {
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<CourseWeek> Weeks { get; set; } = new();
}
public class CourseWeek {
    public int WeekNumber { get; set; }
    public List<CourseDay> Days { get; set; } = new();
}
public class CourseDay {
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<string> Equipments { get; set; } = new();
    public List<CourseSection> Sections { get; set; } = new();
}
public class CourseSection {
    public string Name { get; set; } = string.Empty;
    public List<CourseTraining> Trainings { get; set; } = new();
}
public class CourseTraining {
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TrainingId { get; set; }
    public int Order { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public double Weight { get; set; }
    public int DurationSeconds { get; set; }
    public int RestSeconds { get; set; }
}
""")

write_file("src/AzureTest.Domain/Documents/Gym/UserCourse.cs", """
using System;
using System.Collections.Generic;
namespace AzureTest.Domain.Documents.Gym;
public class UserCourse {
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public Guid OriginalCourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<CourseWeek> Weeks { get; set; } = new();
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public List<Guid> CompletedDayIds { get; set; } = new();
}
""")

write_file("src/AzureTest.Domain/Documents/Gym/UserTrainingSession.cs", """
using System;
using System.Collections.Generic;
namespace AzureTest.Domain.Documents.Gym;
public class UserTrainingSession {
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public Guid UserCourseId { get; set; }
    public Guid CourseDayId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalDurationSeconds { get; set; }
    public double TotalCaloriesBurned { get; set; }
    public List<TrainingLog> Logs { get; set; } = new();
}
public class TrainingLog {
    public Guid TrainingId { get; set; }
    public int SetsCompleted { get; set; }
    public int RepsCompleted { get; set; }
    public int DurationCompletedSeconds { get; set; }
    public double WeightUsed { get; set; }
    public double CaloriesBurned { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
""")

# 3. Interfaces
write_file("src/AzureTest.Application/Interfaces/Data/NoSql/ITrainingRepository.cs", """
using AzureTest.Domain.Documents.Gym;
namespace AzureTest.Application.Interfaces.Data.NoSql;
public interface ITrainingRepository : IDocumentRepository<Training> { }
""")
write_file("src/AzureTest.Application/Interfaces/Data/NoSql/ITrainingCourseRepository.cs", """
using AzureTest.Domain.Documents.Gym;
namespace AzureTest.Application.Interfaces.Data.NoSql;
public interface ITrainingCourseRepository : IDocumentRepository<TrainingCourse> { }
""")
write_file("src/AzureTest.Application/Interfaces/Data/NoSql/IUserCourseRepository.cs", """
using AzureTest.Domain.Documents.Gym;
namespace AzureTest.Application.Interfaces.Data.NoSql;
public interface IUserCourseRepository : IDocumentRepository<UserCourse> { }
""")
write_file("src/AzureTest.Application/Interfaces/Data/NoSql/IUserTrainingSessionRepository.cs", """
using AzureTest.Domain.Documents.Gym;
namespace AzureTest.Application.Interfaces.Data.NoSql;
public interface IUserTrainingSessionRepository : IDocumentRepository<UserTrainingSession> { }
""")

# 4. Implementations
write_file("src/AzureTest.Persistence/Repositories/NoSql/TrainingRepository.cs", """
using AzureTest.Domain.Documents.Gym;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;
namespace AzureTest.Persistence.Repositories.NoSql;
public class TrainingRepository : DocumentRepository<Training>, ITrainingRepository {
    public TrainingRepository(IMongoDatabase database) : base(database, "Trainings") { }
}
""")
write_file("src/AzureTest.Persistence/Repositories/NoSql/TrainingCourseRepository.cs", """
using AzureTest.Domain.Documents.Gym;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;
namespace AzureTest.Persistence.Repositories.NoSql;
public class TrainingCourseRepository : DocumentRepository<TrainingCourse>, ITrainingCourseRepository {
    public TrainingCourseRepository(IMongoDatabase database) : base(database, "TrainingCourses") { }
}
""")
write_file("src/AzureTest.Persistence/Repositories/NoSql/UserCourseRepository.cs", """
using AzureTest.Domain.Documents.Gym;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;
namespace AzureTest.Persistence.Repositories.NoSql;
public class UserCourseRepository : DocumentRepository<UserCourse>, IUserCourseRepository {
    public UserCourseRepository(IMongoDatabase database) : base(database, "UserCourses") { }
}
""")
write_file("src/AzureTest.Persistence/Repositories/NoSql/UserTrainingSessionRepository.cs", """
using AzureTest.Domain.Documents.Gym;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;
namespace AzureTest.Persistence.Repositories.NoSql;
public class UserTrainingSessionRepository : DocumentRepository<UserTrainingSession>, IUserTrainingSessionRepository {
    public UserTrainingSessionRepository(IMongoDatabase database) : base(database, "UserTrainingSessions") { }
}
""")

print("Backend Scaffolded")
