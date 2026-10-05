import re

file = "src/AzureTest.MVC/Controllers/IdeaTrackerController.cs"
with open(file, "r") as f:
    content = f.read()

bad_block = """    [HttpPost]

    [HttpPost]
    [IgnoreAntiforgeryToken]
    [DisplayName("Upload Image")]
    [Description("Execute Upload Image operation for Idea Tracker.")]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        if (image == null || image.Length == 0) return BadRequest();
        
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "ideas");
        Directory.CreateDirectory(uploadsFolder);
        
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }
        
        return Json(new { location = $"/uploads/ideas/{fileName}" });
    }"""

good_block = """    [HttpPost]
    [IgnoreAntiforgeryToken]
    [DisplayName("Upload Image")]
    [Description("Execute Upload Image operation for Idea Tracker.")]
    public async Task<IActionResult> UploadImage(IFormFile? file, IFormFile? image)
    {
        var uploadedFile = file ?? image;
        if (uploadedFile == null || uploadedFile.Length == 0) return BadRequest(new { error = "No file uploaded." });
        
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "ideas");
        Directory.CreateDirectory(uploadsFolder);
        
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await uploadedFile.CopyToAsync(stream);
        }
        
        return Json(new { location = $"/uploads/ideas/{fileName}" });
    }"""

if bad_block in content:
    content = content.replace(bad_block, good_block)
    with open(file, "w") as f:
        f.write(content)
    print("Replaced successfully")
else:
    print("Could not find the block to replace")
