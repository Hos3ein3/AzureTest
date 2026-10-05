import re

uow_impl = "src/AzureTest.Persistence/Repositories/NoSql/NoSqlUnitOfWork.cs"
with open(uow_impl, "r") as f:
    content = f.read()

# Add private fields
if "_houseSearchHistories" not in content:
    content = content.replace(
        "private IIdeaSectionRepository? _ideaSections;",
        "private IIdeaSectionRepository? _ideaSections;\n    private IHouseSearchHistoryRepository? _houseSearchHistories;\n    private IHouseCrawlerCredentialRepository? _houseCrawlerCredentials;"
    )

# Add public properties
if "public IHouseSearchHistoryRepository HouseSearchHistories" not in content:
    content = content.replace(
        "public IIdeaSectionRepository IdeaSections => _ideaSections ??= new IdeaSectionRepository(_database);",
        "public IIdeaSectionRepository IdeaSections => _ideaSections ??= new IdeaSectionRepository(_database);\n    public IHouseSearchHistoryRepository HouseSearchHistories => _houseSearchHistories ??= new HouseSearchHistoryRepository(_database);\n    public IHouseCrawlerCredentialRepository HouseCrawlerCredentials => _houseCrawlerCredentials ??= new HouseCrawlerCredentialRepository(_database);"
    )

with open(uow_impl, "w") as f:
    f.write(content)
