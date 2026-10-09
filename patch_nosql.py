import re

# 1. Update INoSqlUnitOfWork
path1 = "src/AzureTest.Application/Interfaces/Data/NoSql/INoSqlUnitOfWork.cs"
with open(path1, "r") as f: content1 = f.read()

# Add IPageRepository
content1 = content1.replace(
    "namespace AzureTest.Application.Interfaces.Data.NoSql;",
    "namespace AzureTest.Application.Interfaces.Data.NoSql;\nusing AzureTest.Domain.Documents.Pages;"
)
content1 = content1.replace(
    "public interface INoSqlUnitOfWork",
    "public interface IPageRepository : IDocumentRepository<PageDocument> { }\n\npublic interface INoSqlUnitOfWork"
)
content1 = content1.replace(
    "IIdeaPageRepository IdeaPages { get; }",
    "IIdeaPageRepository IdeaPages { get; }\n    IPageRepository Pages { get; }"
)
with open(path1, "w") as f: f.write(content1)

# 2. Update NoSqlUnitOfWork
path2 = "src/AzureTest.Persistence/Repositories/NoSql/NoSqlUnitOfWork.cs"
with open(path2, "r") as f: content2 = f.read()

content2 = content2.replace(
    "namespace AzureTest.Persistence.Repositories.NoSql;",
    "namespace AzureTest.Persistence.Repositories.NoSql;\nusing AzureTest.Application.Interfaces.Data.NoSql;\nusing AzureTest.Domain.Documents.Pages;"
)
content2 = content2.replace(
    "public class NoSqlUnitOfWork",
    "public class PageRepository : DocumentRepository<PageDocument>, IPageRepository { public PageRepository(IMongoDatabase db) : base(db, \"Pages\") { } }\n\npublic class NoSqlUnitOfWork"
)
content2 = content2.replace(
    "public IIdeaPageRepository IdeaPages { get; }",
    "public IIdeaPageRepository IdeaPages { get; }\n    public IPageRepository Pages { get; }"
)
content2 = content2.replace(
    "IdeaPages = new IdeaPageRepository(database);",
    "IdeaPages = new IdeaPageRepository(database);\n        Pages = new PageRepository(database);"
)
with open(path2, "w") as f: f.write(content2)

# 3. Update MongoDbClassMaps
path3 = "src/AzureTest.Persistence/Configurations/MongoDbClassMaps.cs"
with open(path3, "r") as f: content3 = f.read()

content3 = content3.replace(
    "namespace AzureTest.Persistence.Configurations;",
    "namespace AzureTest.Persistence.Configurations;\nusing AzureTest.Domain.Documents.Pages;"
)
content3 = content3.replace(
    "RegisterClassMap<IdeaPage>",
    "RegisterClassMap<PageDocument>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });\n        RegisterClassMap<PageComment>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });\n        RegisterClassMap<IdeaPage>"
)
with open(path3, "w") as f: f.write(content3)

print("NoSql infrastructure patched.")
