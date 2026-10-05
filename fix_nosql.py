import re

# Update INoSqlUnitOfWork.cs
uow_interface = "src/AzureTest.Application/Interfaces/Data/NoSql/INoSqlUnitOfWork.cs"
with open(uow_interface, "r") as f:
    content = f.read()

content = content.replace(
    "Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);",
    "IHouseSearchHistoryRepository HouseSearchHistories { get; }\n    IHouseCrawlerCredentialRepository HouseCrawlerCredentials { get; }\n\n    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);"
)

with open(uow_interface, "w") as f:
    f.write(content)

# Update NoSqlUnitOfWork.cs
uow_impl = "src/AzureTest.Persistence/Repositories/NoSql/NoSqlUnitOfWork.cs"
with open(uow_impl, "r") as f:
    content = f.read()

if "IHouseSearchHistoryRepository" not in content:
    content = content.replace(
        "public IIdeaSectionRepository IdeaSections { get; }",
        "public IIdeaSectionRepository IdeaSections { get; }\n    public IHouseSearchHistoryRepository HouseSearchHistories { get; }\n    public IHouseCrawlerCredentialRepository HouseCrawlerCredentials { get; }"
    )
    content = content.replace(
        "IIdeaSectionRepository ideaSections)",
        "IIdeaSectionRepository ideaSections,\n        IHouseSearchHistoryRepository houseSearchHistories,\n        IHouseCrawlerCredentialRepository houseCrawlerCredentials)"
    )
    content = content.replace(
        "IdeaSections = ideaSections;",
        "IdeaSections = ideaSections;\n        HouseSearchHistories = houseSearchHistories;\n        HouseCrawlerCredentials = houseCrawlerCredentials;"
    )
    with open(uow_impl, "w") as f:
        f.write(content)
