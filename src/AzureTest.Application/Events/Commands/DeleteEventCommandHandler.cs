using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Events.Commands;

public class DeleteEventCommandHandler : BaseCommandHandler<DeleteEventCommand, Result>
{

    private readonly ICurrentUser _currentUser;

    public DeleteEventCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, ILogger<DeleteEventCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(DeleteEventCommand command, CancellationToken cancellationToken)
    {
        var ev = await UnitOfWork.Events.GetByIdAsync(command.Id);
        if (ev == null || ev.UserId != _currentUser.Id) return Result.Failure("Event not found.");

        ev.IsDeleted = true;
        ev.DeletedOn = DateTime.UtcNow;
        ev.DeletedBy = _currentUser.Id.ToString();
        
        return Result.Success();
    }
}
