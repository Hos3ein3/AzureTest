import re

file_path = "src/AzureTest.Persistence/Configurations/NoSql/MongoDbClassMaps.cs"
with open(file_path, "r") as f:
    content = f.read()

if "HouseSearchHistory" not in content:
    new_maps = """                BsonClassMap.RegisterClassMap<HouseSearchHistory>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(c => c.Id);
                    cm.SetIgnoreExtraElements(true);
                });

                BsonClassMap.RegisterClassMap<HouseCrawlerCredential>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(c => c.Id);
                    cm.SetIgnoreExtraElements(true);
                });\n\n"""
    
    content = content.replace(
        "BsonClassMap.RegisterClassMap<JobSearchHistory>(cm =>",
        new_maps + "                BsonClassMap.RegisterClassMap<JobSearchHistory>(cm =>"
    )
    with open(file_path, "w") as f:
        f.write(content)
