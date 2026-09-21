using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Abstractions;

/// <summary>
/// Base handler for queries. Provides structured logging and error handling.
/// Queries are read-only — no transaction or rollback is needed.
/// </summary>
public abstract class BaseQueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    protected readonly ILogger Logger;

    protected BaseQueryHandler(ILogger logger)
    {
        Logger = logger;
    }

    public async Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        var queryName = typeof(TQuery).Name;

        Logger.LogInformation("Handling query {QueryName}", queryName);

        try
        {
            var result = await ExecuteAsync(query, cancellationToken);
            Logger.LogInformation("Query {QueryName} completed successfully", queryName);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Query {QueryName} failed: {Message}", queryName, ex.Message);
            throw;
        }
    }

    protected abstract Task<TResponse> ExecuteAsync(TQuery query, CancellationToken cancellationToken);
}
