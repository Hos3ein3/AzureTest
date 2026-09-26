using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;

using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tasks.Commands;


public record DeleteTaskCommand(Guid Id) : ICommand<Result>;


public class DeleteTaskCommandHandler : BaseCommandHandler<DeleteTaskCommand, Result>
{

    private readonly ICurrentUser _currentUser;

    public DeleteTaskCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, ILogger<DeleteTaskCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        var t = await UnitOfWork.Tasks.GetByIdAsync(command.Id);
        if (t == null || t.UserId != _currentUser.Id) return Result.Failure("Task not found.");

        t.IsDeleted = true;
        t.DeletedOn = DateTime.UtcNow;
        t.DeletedBy = _currentUser.Id.ToString();
        
        return Result.Success();
    }
}
