import re

path = "src/AzureTest.MVC/Controllers/GymCoursesController.cs"
with open(path, "r") as f: c = f.read()

if "IBlobService" not in c:
    c = "using AzureTest.Application.Interfaces.Services;\nusing Microsoft.AspNetCore.Http;\n" + c
    c = c.replace("GetAllTrainingsQueryHandler _getAllTrainingsQuery;", "GetAllTrainingsQueryHandler _getAllTrainingsQuery;\n    private readonly IBlobService _blobService;")
    c = c.replace("GetAllTrainingsQueryHandler getAllTrainingsQuery)", "GetAllTrainingsQueryHandler getAllTrainingsQuery, IBlobService blobService)")
    c = c.replace("_getAllTrainingsQuery = getAllTrainingsQuery;", "_getAllTrainingsQuery = getAllTrainingsQuery;\n        _blobService = blobService;")

upload_action = """
    [HttpPost]
    public async Task<IActionResult> UploadMedia(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded");
        var ext = System.IO.Path.GetExtension(file.FileName);
        var blobName = $"{Guid.NewGuid()}{ext}";
        using var stream = file.OpenReadStream();
        var url = await _blobService.UploadFileAsync("gym", blobName, stream, file.ContentType);
        return Json(new { success = true, url = url });
    }
"""

if "UploadMedia" not in c:
    c = c.replace("public async Task<IActionResult> Create([FromBody] TrainingCourse model)", upload_action + "\n    public async Task<IActionResult> Create([FromBody] TrainingCourse model)")

with open(path, "w") as f: f.write(c)
print("GymCoursesController patched")
