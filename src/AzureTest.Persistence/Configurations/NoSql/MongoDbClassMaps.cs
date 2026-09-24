using AzureTest.Domain.Documents;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace AzureTest.Persistence.Configurations.NoSql;

/// <summary>
/// Configures MongoDB BSON class mappings for all document types.
/// Must be registered once at startup before any MongoDB operations.
/// </summary>
public static class MongoDbClassMaps
{
    private static bool _registered;
    private static readonly object _lock = new();

    public static void Register()
    {
        lock (_lock)
        {
            if (_registered) return;

            // Ensure Guid values are serialized as strings (not binary) in MongoDB
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            // Map ResumeFiles document
            if (!BsonClassMap.IsClassMapRegistered(typeof(ResumeFiles)))
            {
                BsonClassMap.RegisterClassMap<ResumeFiles>(cm =>
                {
                    cm.AutoMap();

                    // Map Id property to MongoDB _id field
                    cm.MapIdMember(c => c.Id)
                      .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    // Ensure ResumeId is also serialized as a standard Guid string
                    cm.MapMember(c => c.ResumeId)
                      .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.SetIgnoreExtraElements(true);
                });
            }

            // Map StatusLog document
            if (!BsonClassMap.IsClassMapRegistered(typeof(StatusLog)))
            {
                BsonClassMap.RegisterClassMap<StatusLog>(cm =>
                {
                    cm.AutoMap();

                    // Map Id property to MongoDB _id field
                    cm.MapIdMember(c => c.Id)
                      .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    // Ensure ResumeId is also serialized as a standard Guid string
                    cm.MapMember(c => c.ResumeId)
                      .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.SetIgnoreExtraElements(true);
                });
            }
            
            if (!BsonClassMap.IsClassMapRegistered(typeof(Company)))
            {
                BsonClassMap.RegisterClassMap<Company>(cm =>
                {
                    cm.AutoMap();

                    // Map Id property to MongoDB _id field
                    cm.MapIdMember(c => c.Id)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    

                    cm.SetIgnoreExtraElements(true);
                });
            }

            _registered = true;
        }
    }
}
