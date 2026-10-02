import re

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaPageController.cs', 'r') as f:
    content = f.read()

details_action = """
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var result = await _queries.GetIdeaPageById.HandleAsync(new GetIdeaPageByIdQuery(_currentUser.Id, id), ct);
        if (result.IsFailure) return NotFound();

        // Get parent Idea
        var ideaResult = await _ideaQueries.GetIdeaById.HandleAsync(new GetIdeaByIdQuery(_currentUser.Id, result.Value.IdeaId), ct);
        if (ideaResult.IsSuccess) ViewBag.Idea = ideaResult.Value;

        // Get all pages for sidebar
        var pagesResult = await _queries.GetIdeaPages.HandleAsync(new GetIdeaPagesQuery(_currentUser.Id, result.Value.IdeaId), ct);
        if (pagesResult.IsSuccess) ViewBag.Pages = pagesResult.Value;

        return View(result.Value);
    }
"""

if "public async Task<IActionResult> Details" not in content:
    content = content.replace("[HttpGet]\n    public IActionResult Create", details_action + "\n    [HttpGet]\n    public IActionResult Create")
    with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaPageController.cs', 'w') as f:
        f.write(content)

