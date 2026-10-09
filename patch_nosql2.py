import os

file_i = "src/AzureTest.Application/Interfaces/Data/NoSql/INoSqlUnitOfWork.cs"
with open(file_i, "r") as f:
    ci = f.read()
if "ITrainingRepository" not in ci:
    ci = ci.replace("Task<bool> SaveChangesAsync", "ITrainingRepository Trainings { get; }\n    ITrainingCourseRepository TrainingCourses { get; }\n    IUserCourseRepository UserCourses { get; }\n    IUserTrainingSessionRepository UserTrainingSessions { get; }\n\n    Task<bool> SaveChangesAsync")
    with open(file_i, "w") as f:
        f.write(ci)

file_c = "src/AzureTest.Persistence/Repositories/NoSql/NoSqlUnitOfWork.cs"
with open(file_c, "r") as f:
    cc = f.read()
if "ITrainingRepository" not in cc:
    cc = cc.replace("public IIdeaSectionRepository IdeaSections { get; }", "public IIdeaSectionRepository IdeaSections { get; }\n    public ITrainingRepository Trainings { get; }\n    public ITrainingCourseRepository TrainingCourses { get; }\n    public IUserCourseRepository UserCourses { get; }\n    public IUserTrainingSessionRepository UserTrainingSessions { get; }")
    cc = cc.replace("IdeaSections = new IdeaSectionRepository(database);", "IdeaSections = new IdeaSectionRepository(database);\n        Trainings = new TrainingRepository(database);\n        TrainingCourses = new TrainingCourseRepository(database);\n        UserCourses = new UserCourseRepository(database);\n        UserTrainingSessions = new UserTrainingSessionRepository(database);")
    with open(file_c, "w") as f:
        f.write(cc)

file_m = "src/AzureTest.Persistence/Configurations/NoSql/MongoDbClassMaps.cs"
with open(file_m, "r") as f:
    cm = f.read()
if "AzureTest.Domain.Documents.Gym" not in cm:
    cm = cm.replace("using MongoDB.Bson.Serialization.IdGenerators;", "using MongoDB.Bson.Serialization.IdGenerators;\nusing AzureTest.Domain.Documents.Gym;")
    target = "BsonClassMap.RegisterClassMap<IdeaPage>(cm =>"
    cm = cm.replace(target, "BsonClassMap.RegisterClassMap<Training>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });\n        BsonClassMap.RegisterClassMap<TrainingCourse>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });\n        BsonClassMap.RegisterClassMap<UserCourse>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });\n        BsonClassMap.RegisterClassMap<UserTrainingSession>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });\n        " + target)
    with open(file_m, "w") as f:
        f.write(cm)

print("NoSql Patched")
