using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AzureTest.Infrastructure.Jobs;

public class TenantBackfillJob : ITenantBackfillJob
{
    private readonly AzureTestDbContext _dbContext;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ILogger<TenantBackfillJob> _logger;

    public TenantBackfillJob(
        AzureTestDbContext dbContext,
        IMongoDatabase mongoDatabase,
        ILogger<TenantBackfillJob> logger)
    {
        _dbContext = dbContext;
        _mongoDatabase = mongoDatabase;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Beginning TenantId backfill job for legacy records in PostgreSQL and MongoDB.");

        await BackfillPostgresTablesAsync(cancellationToken);
        await BackfillMongoCollectionsAsync(cancellationToken);

        _logger.LogInformation("TenantId backfill job completed successfully.");
    }

    private async Task BackfillPostgresTablesAsync(CancellationToken cancellationToken)
    {
        var directTables = new[]
        {
            "Debts",
            "Tasks",
            "Expenses",
            "HabitTrackers",
            "Events",
            "Resumes",
            "Tickets",
            "PomodoroSessions",
            "PomodoroSettings",
            "SampleNotes"
        };

        const int batchSize = 500;

        foreach (var table in directTables)
        {
            var totalUpdated = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                var sql = $@"
                    WITH batch AS (
                        SELECT ""Id"", ""UserId""
                        FROM ""{table}""
                        WHERE ""TenantId"" = '00000000-0000-0000-0000-000000000000'
                        LIMIT {batchSize}
                    )
                    UPDATE ""{table}"" t
                    SET ""TenantId"" = b.""UserId""
                    FROM batch b
                    WHERE t.""Id"" = b.""Id"";";

                try
                {
                    var affected = await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
                    totalUpdated += affected;
                    if (affected < batchSize) break;
                    await Task.Delay(50, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not backfill table {Table}. It might not exist or lacks UserId.", table);
                    break;
                }
            }

            if (totalUpdated > 0)
            {
                _logger.LogInformation("Backfilled {Count} legacy rows in PostgreSQL table {Table}.", totalUpdated, table);
            }
        }

        // Child tables: HabitLogs (from HabitTrackers) and TicketMessages (from Tickets)
        try
        {
            var habitLogSql = @"
                UPDATE ""HabitLogs"" hl
                SET ""TenantId"" = ht.""UserId""
                FROM ""HabitTrackers"" ht
                WHERE hl.""HabitTrackerId"" = ht.""Id""
                  AND hl.""TenantId"" = '00000000-0000-0000-0000-000000000000';";
            var habitLogsUpdated = await _dbContext.Database.ExecuteSqlRawAsync(habitLogSql, cancellationToken);
            if (habitLogsUpdated > 0)
            {
                _logger.LogInformation("Backfilled {Count} legacy rows in HabitLogs from parent HabitTrackers.", habitLogsUpdated);
            }

            var ticketMsgSql = @"
                UPDATE ""TicketMessages"" tm
                SET ""TenantId"" = t.""UserId""
                FROM ""Tickets"" t
                WHERE tm.""TicketId"" = t.""Id""
                  AND tm.""TenantId"" = '00000000-0000-0000-0000-000000000000';";
            var ticketMsgsUpdated = await _dbContext.Database.ExecuteSqlRawAsync(ticketMsgSql, cancellationToken);
            if (ticketMsgsUpdated > 0)
            {
                _logger.LogInformation("Backfilled {Count} legacy rows in TicketMessages from parent Tickets.", ticketMsgsUpdated);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error backfilling relational child tables.");
        }
    }

    private async Task BackfillMongoCollectionsAsync(CancellationToken cancellationToken)
    {
        var collectionsWithUserId = new[]
        {
            "DebtPerson",
            "UserWeightLog",
            "UserTrainingSession",
            "UserGymCourse",
            "ExpenseCategory",
            "Idea",
            "ResumeFiles",
            "JobSearchHistory",
            "HouseSearchHistory",
            "HouseCrawlerCredential",
            "UserDashboard",
            "UserPreference",
            "RecipeDocument"
        };

        foreach (var colName in collectionsWithUserId)
        {
            try
            {
                var collection = _mongoDatabase.GetCollection<BsonDocument>(colName);
                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Exists("UserId", true),
                    Builders<BsonDocument>.Filter.Ne("UserId", BsonNull.Value),
                    Builders<BsonDocument>.Filter.Or(
                        Builders<BsonDocument>.Filter.Exists("TenantId", false),
                        Builders<BsonDocument>.Filter.Eq("TenantId", BsonNull.Value),
                        Builders<BsonDocument>.Filter.Eq("TenantId", Guid.Empty.ToString()),
                        Builders<BsonDocument>.Filter.Eq("TenantId", new BsonBinaryData(Guid.Empty, GuidRepresentation.Standard))
                    )
                );

                var totalModified = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    var docs = await collection.Find(filter).Limit(500).ToListAsync(cancellationToken);
                    if (docs.Count == 0) break;

                    var writes = new System.Collections.Generic.List<WriteModel<BsonDocument>>();
                    foreach (var doc in docs)
                    {
                        var id = doc["_id"];
                        var userId = doc["UserId"];
                        var update = Builders<BsonDocument>.Update.Set("TenantId", userId);
                        writes.Add(new UpdateOneModel<BsonDocument>(Builders<BsonDocument>.Filter.Eq("_id", id), update));
                    }

                    var result = await collection.BulkWriteAsync(writes, cancellationToken: cancellationToken);
                    totalModified += (int)result.ModifiedCount;
                    if (docs.Count < 500) break;
                    await Task.Delay(50, cancellationToken);
                }

                if (totalModified > 0)
                {
                    _logger.LogInformation("Backfilled {Count} legacy documents in MongoDB collection {Collection}.",
                        totalModified, colName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not backfill MongoDB collection {Collection}.", colName);
            }
        }
    }
}
