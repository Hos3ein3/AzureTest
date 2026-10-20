using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Behaviors;

/// <summary>
/// Decorator that automatically records audit logs for all ICommandHandler&lt;TCommand&gt; executions.
/// </summary>
public class AuditCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _inner;
    private readonly IAuditLogService _auditLogService;

    public AuditCommandHandlerDecorator(ICommandHandler<TCommand> inner, IAuditLogService auditLogService)
    {
        _inner = inner;
        _auditLogService = auditLogService;
    }

    public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        Result result;
        try
        {
            result = await _inner.HandleAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            sw.Stop();
            await _auditLogService.LogCommandAsync(command, sw.ElapsedMilliseconds, false, ex.Message, cancellationToken);
            throw;
        }

        sw.Stop();
        await _auditLogService.LogCommandAsync(command, sw.ElapsedMilliseconds, result.IsSuccess, result.Error, cancellationToken);
        return result;
    }
}

/// <summary>
/// Decorator that automatically records audit logs for all ICommandHandler&lt;TCommand, TResponse&gt; executions.
/// </summary>
public class AuditCommandHandlerDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ICommandHandler<TCommand, TResponse> _inner;
    private readonly IAuditLogService _auditLogService;

    public AuditCommandHandlerDecorator(ICommandHandler<TCommand, TResponse> inner, IAuditLogService auditLogService)
    {
        _inner = inner;
        _auditLogService = auditLogService;
    }

    public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        TResponse response;
        try
        {
            response = await _inner.HandleAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            sw.Stop();
            await _auditLogService.LogCommandAsync(command, sw.ElapsedMilliseconds, false, ex.Message, cancellationToken);
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

        await _auditLogService.LogCommandAsync(command, sw.ElapsedMilliseconds, isSuccess, error, cancellationToken);
        return response;
    }
}
