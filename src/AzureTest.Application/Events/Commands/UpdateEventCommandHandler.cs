using AzureTest.Application.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Events.Commands;

public class UpdateEventCommandHandler : BaseCommandHandler<UpdateEventCommand, Result>
{

    private readonly ICurrentUser _currentUser;

    public UpdateEventCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, ILogger<UpdateEventCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(UpdateEventCommand command, CancellationToken cancellationToken)
    {
        var ev = await UnitOfWork.Events.GetByIdAsync(command.Id);
        if (ev == null || ev.UserId != _currentUser.Id) return Result.Failure("Event not found.");

        ev.Update(command.Title, command.Description, command.StartDate.ToUtc(), command.EndDate.ToUtc(), command.IsAllDay, command.Color);
        
        return Result.Success();
    }
}
