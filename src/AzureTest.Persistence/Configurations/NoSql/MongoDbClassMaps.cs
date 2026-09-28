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

            // Map ResumeStatusLog document
            if (!BsonClassMap.IsClassMapRegistered(typeof(ResumeStatusLog)))
            {
                BsonClassMap.RegisterClassMap<ResumeStatusLog>(cm =>
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

            if (!BsonClassMap.IsClassMapRegistered(typeof(DebtPerson)))
            {
                BsonClassMap.RegisterClassMap<DebtPerson>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(c => c.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                    cm.SetIgnoreExtraElements(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(ExpenseCategory)))
            {
                BsonClassMap.RegisterClassMap<ExpenseCategory>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(c => c.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                    cm.SetIgnoreExtraElements(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(UserPreference)))
            {
                BsonClassMap.RegisterClassMap<UserPreference>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(c => c.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                    cm.SetIgnoreExtraElements(true);
                });
            }

            _registered = true;
        }
    }
}
