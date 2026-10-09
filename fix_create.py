path = "src/AzureTest.Application/Gym/TrainingCourseCommandUnit.cs"
with open(path, "r") as f: c = f.read()
c = c.replace("VideoPath = request.VideoPath ?? new()", "VideoPath = request.VideoPath")
with open(path, "w") as f: f.write(c)
