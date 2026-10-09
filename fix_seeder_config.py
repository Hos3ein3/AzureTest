path = "src/AzureTest.Infrastructure/Seeding/IdentityDataSeeder.cs"
with open(path, "r") as f:
    content = f.read()

content = content.replace("private readonly Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings> _dashboardSettings;", "private readonly Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings> _dashboardSettings;\n    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;")

content = content.replace(
    "Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings> dashboardSettings)",
    "Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings> dashboardSettings, Microsoft.Extensions.Configuration.IConfiguration configuration)"
)
content = content.replace("_dashboardSettings = dashboardSettings;", "_dashboardSettings = dashboardSettings;\n        _configuration = configuration;")

with open(path, "w") as f:
    f.write(content)
