import re

file = "src/AzureTest.Application/Dashboards/Components/Queries/GetAllComponentsQuery.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
"""public record GetAllComponentsQuery(string? Search = null, string? Category = null, string? Sort = null, string? Dir = null, int Page = 1, int PageSize = 10) : IQuery<PagedResult<UnifiedComponentDto>>;""",
"""public record GetAllComponentsQuery(string? Search = null, string? Category = null, string? Type = null, string? Sort = null, string? Dir = null, int Page = 1, int PageSize = 10) : IQuery<PagedResult<UnifiedComponentDto>>;"""
)

content = content.replace(
"""        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var lowerCat = query.Category.ToLower();
            filtered = filtered.Where(c => c.Category.ToLower() == lowerCat);
        }""",
"""        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var lowerCat = query.Category.ToLower();
            filtered = filtered.Where(c => c.Category.ToLower() == lowerCat);
        }

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            var lowerType = query.Type.ToLower();
            filtered = filtered.Where(c => c.Type.ToLower() == lowerType);
        }"""
)

with open(file, "w") as f:
    f.write(content)
