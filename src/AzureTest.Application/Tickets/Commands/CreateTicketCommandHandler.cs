using AzureTest.Application.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tickets.Commands;

public class CreateTicketCommandHandler : BaseCommandHandler<CreateTicketCommand, Result<Guid>>
{

    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notificationService;

    public CreateTicketCommandHandler(ICurrentUser currentUser, INotificationService notificationService, IUnitOfWork unitOfWork, ILogger<CreateTicketCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        var t = Ticket.Create(command.Subject, command.Content, command.Priority, _currentUser.Id);
        UnitOfWork.Tickets.Add(t);
        
        // await _notificationService.SendNotificationAsync("admin@example.com", "New Ticket Created", $"Ticket {t.Id} was created.");
        
        return t.Id;
    }
}
