import re

path = "src/AzureTest.MVC/Controllers/PagesController.cs"
with open(path, "r") as f: c = f.read()

# Add IBlobService to constructor
if "IBlobService" not in c:
    c = "using AzureTest.Application.Interfaces.Services;\n" + c
    c = c.replace("private readonly AnswerCommentCommandHandler _answerComment;", "private readonly AnswerCommentCommandHandler _answerComment;\n    private readonly IBlobService _blobService;")
    c = c.replace("AnswerCommentCommandHandler answerComment)", "AnswerCommentCommandHandler answerComment, IBlobService blobService)")
    c = c.replace("_answerComment = answerComment;", "_answerComment = answerComment;\n        _blobService = blobService;")

# Modify Create
new_create = """
    public async Task<IActionResult> Create(CreatePageDto model, Microsoft.AspNetCore.Http.IFormFile? ImageFile)
    {
        if (!ModelState.IsValid) return View(model);
        
        if (ImageFile != null)
        {
            var ext = System.IO.Path.GetExtension(ImageFile.FileName);
            var blobName = $"{Guid.NewGuid()}{ext}";
            using var stream = ImageFile.OpenReadStream();
            model.ImagePath = await _blobService.UploadFileAsync("pages", blobName, stream, ImageFile.ContentType);
        }

        var cmd = new CreatePageCommand(model.Title, model.Slug, model.Content, model.ImagePath);
        var res = await _create.HandleAsync(cmd);
        if (res.IsSuccess) return RedirectToAction("Index");
        ModelState.AddModelError("", res.Error ?? "Error");
        return View(model);
    }
"""
c = re.sub(r'public async Task<IActionResult> Create\(CreatePageDto model\)\s*\{[^\}]+\}', new_create.strip(), c, flags=re.DOTALL)

# Modify Edit
new_edit = """
    public async Task<IActionResult> Edit(UpdatePageDto model, Microsoft.AspNetCore.Http.IFormFile? ImageFile)
    {
        if (!ModelState.IsValid) return View(model);

        if (ImageFile != null)
        {
            var ext = System.IO.Path.GetExtension(ImageFile.FileName);
            var blobName = $"{Guid.NewGuid()}{ext}";
            using var stream = ImageFile.OpenReadStream();
            model.ImagePath = await _blobService.UploadFileAsync("pages", blobName, stream, ImageFile.ContentType);
        }

        var cmd = new UpdatePageCommand(model.Id, model.Title, model.Slug, model.Content, model.ImagePath, model.Status);
        var res = await _update.HandleAsync(cmd);
        if (res.IsSuccess) return RedirectToAction("Index");
        ModelState.AddModelError("", res.Error ?? "Error");
        return View(model);
    }
"""
c = re.sub(r'public async Task<IActionResult> Edit\(UpdatePageDto model\)\s*\{[^\}]+\}', new_edit.strip(), c, flags=re.DOTALL)

with open(path, "w") as f: f.write(c)
print("PagesController patched for image upload")
