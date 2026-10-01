using AzureTest.Application.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;

using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Tasks.Commands;

public record CreateTaskCommand(string Title, string? Description, DateTime? StartDate, DateTime? EndDate, string? Priority) : ICommand<Result<Guid>>;

public class CreateTaskCommandHandler : BaseCommandHandler<CreateTaskCommand, Result<Guid>>
{

    private readonly ICurrentUser _currentUser;

    public CreateTaskCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, ILogger<CreateTaskCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var t = TaskItem.Create(command.Title, command.Description, command.StartDate?.ToUniversalTime(), command.EndDate?.ToUniversalTime(), command.Priority, _currentUser.Id);
        UnitOfWork.Tasks.Add(t);
        return await Task.FromResult(t.Id);
    }
}
