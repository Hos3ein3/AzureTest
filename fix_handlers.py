import re

# Fix PageQueryUnit
path1 = "src/AzureTest.Application/Pages/PageQueryUnit.cs"
with open(path1, "r") as f: c1 = f.read()
c1 = c1.replace("BaseQueryHandler<", "IQueryHandler<")
c1 = c1.replace("public override async Task<", "public async Task<")
with open(path1, "w") as f: f.write(c1)

# Fix PageCommandUnit
path2 = "src/AzureTest.Application/Pages/PageCommandUnit.cs"
with open(path2, "r") as f: c2 = f.read()
c2 = c2.replace("BaseCommandHandler<", "ICommandHandler<")
c2 = c2.replace("public override async Task<", "public async Task<")
with open(path2, "w") as f: f.write(c2)

print("Handlers fixed.")
