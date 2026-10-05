import re

file = "src/AzureTest.Infrastructure/Seeding/IdentityDataSeeder.cs"
with open(file, "r") as f:
    content = f.read()

# Update json configs loop to set IsResizable = true (since it defaults to true anyway, let's explicit it, maybe set limits if needed?)
content = content.replace(
"""        foreach (var j in jsonConfigs)
        {
            await _noSqlUnitOfWork.DashboardRawComponents.AddAsync(new DashboardRawComponent
            {
                Name = j.Name,
                Type = "JsonConfig",
                Category = "Analytics",
                JsonConfiguration = $$"\"\"\"
                {
                  "title": "{{j.Name}}",
                  "chartType": "{{j.Type}}",
                  "themeColor": "{{j.Color}}",
                  "endpointUrl": "{{j.Url}}"
                }
                \"\"\"
            });
        }""",
"""        foreach (var j in jsonConfigs)
        {
            await _noSqlUnitOfWork.DashboardRawComponents.AddAsync(new DashboardRawComponent
            {
                Name = j.Name,
                Type = "JsonConfig",
                Category = "Analytics",
                MinWidth = 2,
                MinHeight = 2,
                IsResizable = true,
                JsonConfiguration = $$"\"\"\"
                {
                  "title": "{{j.Name}}",
                  "chartType": "{{j.Type}}",
                  "themeColor": "{{j.Color}}",
                  "endpointUrl": "{{j.Url}}"
                }
                \"\"\"
            });
        }"""
)

# Update html configs loop
content = content.replace(
"""        foreach (var h in htmlConfigs)
        {
            await _noSqlUnitOfWork.DashboardRawComponents.AddAsync(new DashboardRawComponent
            {
                Name = h.Item1,
                Type = "RawHtml",
                Category = "Widgets",
                HtmlContent = h.Item2
            });
        }""",
"""        foreach (var h in htmlConfigs)
        {
            await _noSqlUnitOfWork.DashboardRawComponents.AddAsync(new DashboardRawComponent
            {
                Name = h.Item1,
                Type = "RawHtml",
                Category = "Widgets",
                HtmlContent = h.Item2,
                MinWidth = 2,
                MinHeight = 2,
                IsResizable = true
            });
        }"""
)

with open(file, "w") as f:
    f.write(content)
