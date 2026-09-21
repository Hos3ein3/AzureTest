using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Abstractions;

/// <summary>
/// Base handler for void commands (no return value).
/// Wraps execution in a database transaction with full rollback support.
/// After a successful commit, collects and exposes domain events for dispatch.
/// </summary>
public abstract class BaseCommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly ILogger Logger;

    protected BaseCommandHandler(IUnitOfWork unitOfWork, ILogger logger)
    {
        UnitOfWork = unitOfWork;
        Logger = logger;
    }

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var commandName = typeof(TCommand).Name;

        Logger.LogInformation("Handling command {CommandName}", commandName);

        try
        {
            await ExecuteAsync(command, cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Command {CommandName} completed successfully", commandName);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Logger.LogWarning(ex, "Command {CommandName} failed due to a concurrency conflict", commandName);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Command {CommandName} failed: {Message}. Changes have been rolled back.", commandName, ex.Message);
            throw;
        }
    }

    protected abstract Task ExecuteAsync(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Base handler for commands that return a value.
/// Wraps execution in a database transaction with full rollback support.
/// After a successful commit, collects and exposes domain events for dispatch.
/// </summary>
public abstract class BaseCommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly ILogger Logger;

    protected BaseCommandHandler(IUnitOfWork unitOfWork, ILogger logger)
    {
        UnitOfWork = unitOfWork;
        Logger = logger;
    }

    public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var commandName = typeof(TCommand).Name;

        Logger.LogInformation("Handling command {CommandName}", commandName);

        try
        {
            var response = await ExecuteAsync(command, cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Command {CommandName} completed successfully", commandName);
            return response;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Logger.LogWarning(ex, "Command {CommandName} failed due to a concurrency conflict", commandName);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Command {CommandName} failed: {Message}. Changes have been rolled back.", commandName, ex.Message);
            throw;
        }
    }

    protected abstract Task<TResponse> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
}
