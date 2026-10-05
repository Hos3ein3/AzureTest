import re

file2 = "src/AzureTest.Infrastructure/Services/CurrentUser.cs"
with open(file2, "r") as f:
    content2 = f.read()

new_method = """
    public bool HasAccess(HttpContext context, string? url = null)
    {
        if (Roles.Contains("Admin")) return true;

        var targetUrl = url ?? context.Request.Path.Value ?? "/";
        if (string.IsNullOrWhiteSpace(targetUrl) || targetUrl == "/")
            targetUrl = "Home/Index";

        var parts = targetUrl.TrimStart('/').Split('?')[0].Split('/');
        var controller = parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : "Home";
        var action = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : "Index";
        
        if (Roles.Contains("User"))
        {
            var options = context.RequestServices.GetService<Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings>>();
            var reserved = options?.Value.ReservedSlugs ?? new System.Collections.Generic.List<string>();
            if (reserved.Contains(controller, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        var permissionString = $"{controller}/{action}";
        return Permissions.Any(p => p.Equals(permissionString, StringComparison.OrdinalIgnoreCase));
    }
}"""

content2 = content2.rsplit("}", 1)
content2 = content2[0] + new_method

with open(file2, "w") as f:
    f.write(content2)

