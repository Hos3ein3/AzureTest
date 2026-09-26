using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tickets.Commands;

public class UpdateTicketStatusCommandHandler : BaseCommandHandler<UpdateTicketStatusCommand, Result>
{

    private readonly ICurrentUser _currentUser;

    public UpdateTicketStatusCommandHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, ILogger<UpdateTicketStatusCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(UpdateTicketStatusCommand command, CancellationToken cancellationToken)
    {
        var t = await UnitOfWork.Tickets.GetByIdAsync(command.TicketId);
        if (t == null) return Result.Failure("Ticket not found.");
        
        bool isAdmin = _currentUser.Roles.Contains("Admin");
        if (t.UserId != _currentUser.Id && !isAdmin) return Result.Failure("Unauthorized.");

        t.UpdateStatus(command.Status);
        
        return Result.Success();
    }
}
