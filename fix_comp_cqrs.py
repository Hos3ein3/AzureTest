import re

file = "src/AzureTest.Application/Dashboards/Components/Queries/GetAllComponentsQuery.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
"""public record GetAllComponentsQuery(string? SearchTerm = null, string? Category = null, string? SortBy = null, bool SortDescending = false, int PageNumber = 1, int PageSize = 10) : IQuery<PagedResult<UnifiedComponentDto>>;""",
"""public record GetAllComponentsQuery(string? Search = null, string? Category = null, string? Sort = null, string? Dir = null, int Page = 1, int PageSize = 10) : IQuery<PagedResult<UnifiedComponentDto>>;""")

content = content.replace("query.SearchTerm", "query.Search")
content = content.replace("query.SortBy", "query.Sort")
content = content.replace("query.SortDescending", 'query.Dir == "desc"')
content = content.replace("query.PageNumber", "query.Page")

with open(file, "w") as f:
    f.write(content)
