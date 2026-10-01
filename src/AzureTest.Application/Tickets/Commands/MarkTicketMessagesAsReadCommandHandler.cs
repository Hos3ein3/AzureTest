using AzureTest.Application.Abstractions;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tickets.Commands;

public class MarkTicketMessagesAsReadCommandHandler : BaseCommandHandler<MarkTicketMessagesAsReadCommand, Result>
{

    private readonly ICurrentUser _currentUser;

    public MarkTicketMessagesAsReadCommandHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, ILogger<MarkTicketMessagesAsReadCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(MarkTicketMessagesAsReadCommand command, CancellationToken cancellationToken)
    {
        var msgs = await UnitOfWork.TicketMessages.GetUnreadMessagesAsync(command.TicketId, _currentUser.Id);
        
        foreach(var m in msgs)
        {
            m.MarkAsRead();
        return Result.Success();
        }
        
        
        return Result.Success();
    }
}
