using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;

using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tasks.Commands;


public record ToggleTaskCommand(Guid Id) : ICommand<Result>;


public class ToggleTaskCommandHandler : BaseCommandHandler<ToggleTaskCommand, Result>
{

    private readonly ICurrentUser _currentUser;

    public ToggleTaskCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, ILogger<ToggleTaskCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(ToggleTaskCommand command, CancellationToken cancellationToken)
    {
        var t = await UnitOfWork.Tasks.GetByIdAsync(command.Id);
        if (t == null || t.UserId != _currentUser.Id) return Result.Failure("Task not found.");

        t.ToggleCompletion();
        
        return Result.Success();
    }
}
