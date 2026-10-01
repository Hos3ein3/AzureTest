using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Abstractions;

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

    public async Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var commandName = typeof(TCommand).Name;
        Logger.LogInformation("Handling command {CommandName}", commandName);

        try
        {
            var result = await ExecuteAsync(command, cancellationToken);
            if (result.IsSuccess)
            {
                await UnitOfWork.SaveChangesAsync(cancellationToken);
            }
            Logger.LogInformation("Command {CommandName} completed successfully", commandName);
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Logger.LogWarning(ex, "Command {CommandName} failed due to a concurrency conflict", commandName);
            return Result.Failure("Concurrency conflict");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Command {CommandName} failed: {Message}. Changes have been rolled back.", commandName, ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    protected abstract Task<Result> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
}

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
            // Assuming TResponse is Result<T> or Result
            var isSuccess = true;
            if (response is Result r && !r.IsSuccess)
            {
                isSuccess = false;
            }

            if (isSuccess)
            {
                await UnitOfWork.SaveChangesAsync(cancellationToken);
            }

            Logger.LogInformation("Command {CommandName} completed successfully", commandName);
            return response;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Logger.LogWarning(ex, "Command {CommandName} failed due to a concurrency conflict", commandName);
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var type = typeof(TResponse).GetGenericArguments()[0];
                var method = typeof(Result<>).MakeGenericType(type).GetMethod("Failure", new[] { typeof(string) });
                return (TResponse)method.Invoke(null, new object[] { "Concurrency conflict" });
            }
            throw; // fallback
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Command {CommandName} failed: {Message}. Changes have been rolled back.", commandName, ex.Message);
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var type = typeof(TResponse).GetGenericArguments()[0];
                var method = typeof(Result<>).MakeGenericType(type).GetMethod("Failure", new[] { typeof(string) });
                return (TResponse)method.Invoke(null, new object[] { ex.Message });
            }
            throw; // fallback
        }
    }

    protected abstract Task<TResponse> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
}
