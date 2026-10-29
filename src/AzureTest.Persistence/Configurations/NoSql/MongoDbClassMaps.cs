

using AzureTest.Domain.Documents;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace AzureTest.Persistence.Configurations.NoSql;
using AzureTest.Domain.Documents.Pages;

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

            if (!BsonClassMap.IsClassMapRegistered(typeof(JobSearchHistory)))
            {
            if (!BsonClassMap.IsClassMapRegistered(typeof(AzureTest.Domain.Documents.Idea)))
            {
                BsonClassMap.RegisterClassMap<AzureTest.Domain.Documents.Idea>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(AzureTest.Domain.Documents.IdeaPage)))
            {
                BsonClassMap.RegisterClassMap<AzureTest.Domain.Documents.IdeaPage>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
            if (!BsonClassMap.IsClassMapRegistered(typeof(AzureTest.Domain.Documents.IdeaSection)))
            {
                BsonClassMap.RegisterClassMap<AzureTest.Domain.Documents.IdeaSection>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }

                                BsonClassMap.RegisterClassMap<HouseSearchHistory>(cm =>
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
                });

                BsonClassMap.RegisterClassMap<JobSearchHistory>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }

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

            if (!BsonClassMap.IsClassMapRegistered(typeof(DashboardComponent)))
            {
                BsonClassMap.RegisterClassMap<DashboardComponent>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(c => c.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                    cm.SetIgnoreExtraElements(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(UserDashboard)))
            {
                BsonClassMap.RegisterClassMap<UserDashboard>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(c => c.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                    cm.SetIgnoreExtraElements(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(DashboardRawComponent)))
            {
                BsonClassMap.RegisterClassMap<DashboardRawComponent>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(c => c.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                    cm.SetIgnoreExtraElements(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(AzureTest.Domain.Documents.Audit.AuditLogDocument)))
            {
                BsonClassMap.RegisterClassMap<AzureTest.Domain.Documents.Audit.AuditLogDocument>(cm =>
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
