using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;

using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tasks.Commands;


public record UpdateTaskCommand(Guid Id, string Title, string? Description, DateTime? DueDate, string? Priority) : ICommand<Result>;


public class UpdateTaskCommandHandler : BaseCommandHandler<UpdateTaskCommand, Result>
{

    private readonly ICurrentUser _currentUser;

    public UpdateTaskCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, ILogger<UpdateTaskCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var t = await UnitOfWork.Tasks.GetByIdAsync(command.Id);
        if (t == null || t.UserId != _currentUser.Id) return Result.Failure("Task not found.");

        t.Update(command.Title, command.Description, command.DueDate, command.Priority);
        
        return Result.Success();
    }
}
