using AzureTest.Application.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Events.Commands;

public class CreateEventCommandHandler : BaseCommandHandler<CreateEventCommand, Result<Guid>>
{

    private readonly ICurrentUser _currentUser;

    public CreateEventCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, ILogger<CreateEventCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateEventCommand command, CancellationToken cancellationToken)
    {
        var ev = Event.Create(command.Title, command.Description, command.StartDate.ToUtc(), command.EndDate.ToUtc(), command.IsAllDay, command.Color, _currentUser.Id);
        UnitOfWork.Events.Add(ev);
        return await Task.FromResult(ev.Id);
    }
}
