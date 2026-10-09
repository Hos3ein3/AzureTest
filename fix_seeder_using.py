path = "src/AzureTest.Infrastructure/Seeding/IdentityDataSeeder.cs"
with open(path, "r") as f:
    content = f.read()

if "using Microsoft.Extensions.Configuration;" not in content:
    content = "using Microsoft.Extensions.Configuration;\n" + content
    
with open(path, "w") as f:
    f.write(content)
