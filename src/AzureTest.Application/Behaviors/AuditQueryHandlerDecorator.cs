using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Behaviors;

/// <summary>
/// Decorator that automatically records audit logs for all IQueryHandler&lt;TQuery, TResponse&gt; executions.
/// </summary>
public class AuditQueryHandlerDecorator<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> _inner;
    private readonly IAuditLogService _auditLogService;

    public AuditQueryHandlerDecorator(IQueryHandler<TQuery, TResponse> inner, IAuditLogService auditLogService)
    {
        _inner = inner;
        _auditLogService = auditLogService;
    }

    public async Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        TResponse response;
        try
        {
            response = await _inner.HandleAsync(query, cancellationToken);
        }
        catch (Exception ex)
        {
            sw.Stop();
            await _auditLogService.LogQueryAsync(query, sw.ElapsedMilliseconds, false, ex.Message, cancellationToken);
            throw;
        }

        sw.Stop();
        var isSuccess = true;
        string? error = null;

        if (response is Result r && !r.IsSuccess)
        {
            isSuccess = false;
            error = r.Error;
        }

        await _auditLogService.LogQueryAsync(query, sw.ElapsedMilliseconds, isSuccess, error, cancellationToken);
        return response;
    }
}
