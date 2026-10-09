import os

file_di = "src/AzureTest.Persistence/DependencyInjection.cs"
with open(file_di, "r") as f:
    di = f.read()

if "ITrainingRepository" not in di:
    di = di.replace("services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();", "services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();\n            services.AddScoped<ITrainingRepository, TrainingRepository>();\n            services.AddScoped<ITrainingCourseRepository, TrainingCourseRepository>();\n            services.AddScoped<IUserCourseRepository, UserCourseRepository>();\n            services.AddScoped<IUserTrainingSessionRepository, UserTrainingSessionRepository>();")
    with open(file_di, "w") as f:
        f.write(di)
print("DI patched")
