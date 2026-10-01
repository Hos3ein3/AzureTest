sed -i '' '/BsonClassMap.RegisterClassMap<JobSearchHistory>/i\
            if (!BsonClassMap.IsClassMapRegistered(typeof(AzureTest.Domain.Documents.Idea)))\
            {\
                BsonClassMap.RegisterClassMap<AzureTest.Domain.Documents.Idea>(cm =>\
                {\
                    cm.AutoMap();\
                    cm.SetIgnoreExtraElements(true);\
                });\
            }\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Persistence/Configurations/NoSql/MongoDbClassMaps.cs
