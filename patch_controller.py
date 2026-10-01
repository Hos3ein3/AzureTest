import re

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaTrackerController.cs', 'r') as f:
    content = f.read()

# Add IWebHostEnvironment and fix Create parameter
inject = """    private readonly IWebHostEnvironment _env;

    public IdeaTrackerController(ICurrentUser currentUser, IIdeaCommandUnit commands, IIdeaQueryUnit queries, IValidationService validation, IWebHostEnvironment env)
    {
        _currentUser = currentUser;
        _commands = commands;
        _queries = queries;
        _validation = validation;
        _env = env;
    }"""
content = re.sub(r'    public IdeaTrackerController\(.*?\).*?_validation = validation;\n    \}', inject, content, flags=re.DOTALL)
content = content.replace('using Microsoft.AspNetCore.Mvc;', 'using Microsoft.AspNetCore.Mvc;\nusing Microsoft.AspNetCore.Hosting;\nusing Microsoft.AspNetCore.Http;\nusing System.IO;')


create_get = """    [HttpGet]
    public IActionResult Create(Guid? parentId)
    {
        return View(new CreateIdeaDto { ParentId = parentId });
    }"""
content = re.sub(r'    \[HttpGet\]\n    public IActionResult Create\(\)\n    \{\n        return View\(new CreateIdeaDto\(\)\);\n    \}', create_get, content)


details_action = """    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var result = await _queries.GetIdeaById.HandleAsync(new GetIdeaByIdQuery(_currentUser.Id, id), ct);
        if (result.IsFailure) return NotFound();
        
        // Fetch Sub-Pages (Ideas where ParentId == id)
        var filter = new IdeaFilter(_currentUser.Id);
        var allIdeasResult = await _queries.GetIdeas.HandleAsync(new GetIdeasQuery(filter), ct);
        if (allIdeasResult.IsSuccess)
        {
            ViewBag.SubPages = allIdeasResult.Value.Where(x => x.ParentId == id).ToList();
        }
        else
        {
            ViewBag.SubPages = new List<IdeaDto>();
        }

        return View(result.Value);
    }"""
content = re.sub(r'    public async Task<IActionResult> Details\(Guid id, CancellationToken ct\)\n    \{.*?return View\(result\.Value\);\n    \}', details_action, content, flags=re.DOTALL)


upload_action = """
    [HttpPost]
    [IgnoreAntiforgeryToken]
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
        
        return Json(new { success = 1, file = new { url = $"/uploads/ideas/{fileName}" } });
    }
"""

content = content.replace('    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)', upload_action + '    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)')

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaTrackerController.cs', 'w') as f:
    f.write(content)
