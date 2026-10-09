path = "src/AzureTest.Application/Gym/TrainingCourseCommandUnit.cs"
with open(path, "r") as f: c = f.read()

c = c.replace("course.Weeks = request.Weeks, ImagePath = request.ImagePath, VideoPath = request.VideoPath ?? new();", 
"""course.Weeks = request.Weeks ?? new();
        course.ImagePath = request.ImagePath;
        course.VideoPath = request.VideoPath;""")

c = c.replace(r"course.Weeks = request.Weeks ?? new();\n        course.ImagePath = request.ImagePath;\n        course.VideoPath = request.VideoPath;", 
"""course.Weeks = request.Weeks ?? new();
        course.ImagePath = request.ImagePath;
        course.VideoPath = request.VideoPath;""")

with open(path, "w") as f: f.write(c)
