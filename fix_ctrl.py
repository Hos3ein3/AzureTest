import re

file = "src/AzureTest.MVC/Controllers/ComponentController.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
"""    public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search = null, string? category = null, string sort = "Name", string dir = "asc", int page = 1)
    {
        var query = new GetAllComponentsQuery(search, category, sort, dir, page, 10);
        var components = await _getAllComponentsQuery.HandleAsync(query, cancellationToken);
        
        ViewBag.CurrentSearch = search;
        ViewBag.Category = category;""",
"""    public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search = null, string? category = null, string? type = null, string sort = "Name", string dir = "asc", int page = 1)
    {
        var query = new GetAllComponentsQuery(search, category, type, sort, dir, page, 10);
        var components = await _getAllComponentsQuery.HandleAsync(query, cancellationToken);
        
        ViewBag.CurrentSearch = search;
        ViewBag.Category = category;
        ViewBag.CurrentType = type;"""
)

with open(file, "w") as f:
    f.write(content)
