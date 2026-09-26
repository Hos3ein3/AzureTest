using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Enums;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tickets.Commands;

public class AddTicketMessageCommandHandler : BaseCommandHandler<AddTicketMessageCommand, Result>
{

    private readonly ICurrentUser _currentUser;

    public AddTicketMessageCommandHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, ILogger<AddTicketMessageCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(AddTicketMessageCommand command, CancellationToken cancellationToken)
    {
        var t = await UnitOfWork.Tickets.GetByIdAsync(command.TicketId);
        if (t == null) return Result.Failure("Ticket not found.");
        
        // Ensure user owns ticket or is admin
        bool isAdmin = _currentUser.Roles.Contains("Admin");
        if (t.UserId != _currentUser.Id && !isAdmin) return Result.Failure("Unauthorized.");

        var msg = TicketMessage.Create(command.TicketId, _currentUser.Id, command.Content);
        
        // If the creator themselves is sending a message, mark it as read immediately
        if (t.UserId == _currentUser.Id)
        {
            msg.MarkAsRead();
            t.UpdateStatus(TicketStatus.Open);
        }
        else
        {
            t.UpdateStatus(TicketStatus.Answered);
        }

        UnitOfWork.TicketMessages.Add(msg);
        
        return Result.Success();
    }
}
