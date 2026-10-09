import os

file_c = "src/AzureTest.Persistence/Repositories/NoSql/NoSqlUnitOfWork.cs"
with open(file_c, "r") as f:
    cc = f.read()

target = "public IIdeaSectionRepository IdeaSections => _ideaSections ??= new IdeaSectionRepository(_database);"
replacement = target + """
    private ITrainingRepository? _trainings;
    private ITrainingCourseRepository? _trainingCourses;
    private IUserCourseRepository? _userCourses;
    private IUserTrainingSessionRepository? _userTrainingSessions;

    public ITrainingRepository Trainings => _trainings ??= new TrainingRepository(_database);
    public ITrainingCourseRepository TrainingCourses => _trainingCourses ??= new TrainingCourseRepository(_database);
    public IUserCourseRepository UserCourses => _userCourses ??= new UserCourseRepository(_database);
    public IUserTrainingSessionRepository UserTrainingSessions => _userTrainingSessions ??= new UserTrainingSessionRepository(_database);
"""

if "public ITrainingRepository Trainings" not in cc:
    cc = cc.replace(target, replacement)
    with open(file_c, "w") as f:
        f.write(cc)
print("Lazy Init patched")
