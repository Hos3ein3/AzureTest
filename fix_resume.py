path = "src/AzureTest.MVC/Controllers/ResumeController.cs"
with open(path, 'r') as f:
    c = f.read()

c = c.replace(
"""        var createUploadDto = new UploadResumeFilesDto(
            id,
            cvFile?.OpenReadStream(), cvFile?.FileName, cvFile?.ContentType,""",
"""        var createUploadDto = new UploadResumeFilesDto(
            id.Value,
            cvFile?.OpenReadStream(), cvFile?.FileName, cvFile?.ContentType,""")

c = c.replace("new { id = result }", "new { id = result.Value }")
c = c.replace("new { id = result.Id }", "new { id = result.Value }")

with open(path, 'w') as f:
    f.write(c)
