path3 = "src/AzureTest.Persistence/Configurations/NoSql/MongoDbClassMaps.cs"
with open(path3, "r") as f: content3 = f.read()

content3 = content3.replace(
    "namespace AzureTest.Persistence.Configurations.NoSql;",
    "namespace AzureTest.Persistence.Configurations.NoSql;\nusing AzureTest.Domain.Documents.Pages;"
)
content3 = content3.replace(
    "RegisterClassMap<IdeaPage>",
    "RegisterClassMap<PageDocument>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });\n        RegisterClassMap<PageComment>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });\n        RegisterClassMap<IdeaPage>"
)
with open(path3, "w") as f: f.write(content3)
print("MongoDbClassMaps patched.")
