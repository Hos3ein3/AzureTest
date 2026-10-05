import re

file = "src/AzureTest.MVC/Controllers/ComponentController.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
"""    public async Task<IActionResult> Index(CancellationToken cancellationToken, string? searchTerm = null, string? category = null, string? sortBy = null, bool sortDescending = false, int pageNumber = 1, int pageSize = 10)
    {
        var query = new GetAllComponentsQuery(searchTerm, category, sortBy, sortDescending, pageNumber, pageSize);
        var components = await _getAllComponentsQuery.HandleAsync(query, cancellationToken);
        
        ViewBag.SearchTerm = searchTerm;
        ViewBag.Category = category;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;""",
"""    public async Task<IActionResult> Index(CancellationToken cancellationToken, string? search = null, string? category = null, string sort = "Name", string dir = "asc", int page = 1)
    {
        var query = new GetAllComponentsQuery(search, category, sort, dir, page, 10);
        var components = await _getAllComponentsQuery.HandleAsync(query, cancellationToken);
        
        ViewBag.CurrentSearch = search;
        ViewBag.Category = category;
        ViewBag.CurrentSort = sort;
        ViewBag.CurrentDir = dir;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = components.TotalPages;"""
)

with open(file, "w") as f:
    f.write(content)
