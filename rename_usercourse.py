import os
import glob

# 1. Rename files
def rename_file(old, new):
    if os.path.exists(old):
        os.rename(old, new)

rename_file("src/AzureTest.Domain/Documents/Gym/UserCourse.cs", "src/AzureTest.Domain/Documents/Gym/UserGymCourse.cs")
rename_file("src/AzureTest.Application/Interfaces/Data/NoSql/IUserCourseRepository.cs", "src/AzureTest.Application/Interfaces/Data/NoSql/IUserGymCourseRepository.cs")
rename_file("src/AzureTest.Persistence/Repositories/NoSql/UserCourseRepository.cs", "src/AzureTest.Persistence/Repositories/NoSql/UserGymCourseRepository.cs")
rename_file("src/AzureTest.Application/Gym/UserCourseCommandUnit.cs", "src/AzureTest.Application/Gym/UserGymCourseCommandUnit.cs")
rename_file("src/AzureTest.Application/Gym/UserCourseQueryUnit.cs", "src/AzureTest.Application/Gym/UserGymCourseQueryUnit.cs")

# 2. Replace content in all relevant files
search_dirs = [
    "src/AzureTest.Domain/Documents/Gym",
    "src/AzureTest.Application/Interfaces/Data/NoSql",
    "src/AzureTest.Persistence/Repositories/NoSql",
    "src/AzureTest.Persistence/Configurations/NoSql",
    "src/AzureTest.Application/Gym",
    "src/AzureTest.MVC/Controllers",
    "src/AzureTest.MVC/Views/Workout",
    "src/AzureTest.Persistence"
]

for d in search_dirs:
    for root, dirs, files in os.walk(d):
        for file in files:
            if file.endswith(".cs") or file.endswith(".cshtml"):
                path = os.path.join(root, file)
                with open(path, "r") as f:
                    content = f.read()
                
                if "UserCourse" in content or "userCourse" in content or "userCourses" in content or "UserCourses" in content:
                    new_content = content.replace("UserCourse", "UserGymCourse")
                    new_content = new_content.replace("userCourse", "userGymCourse")
                    new_content = new_content.replace("UserCourses", "UserGymCourses")
                    new_content = new_content.replace("userCourses", "userGymCourses")
                    with open(path, "w") as f:
                        f.write(new_content)

print("Rename complete")
