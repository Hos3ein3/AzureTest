using AzureTest.Application.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Habits.Commands;

public class CreateHabitCommandHandler : BaseCommandHandler<CreateHabitCommand, Result<Guid>>
{

    private readonly ICurrentUser _currentUser;

    public CreateHabitCommandHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, ILogger<CreateHabitCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override Task<Result<Guid>> ExecuteAsync(CreateHabitCommand command, CancellationToken cancellationToken)
    {
        var h = HabitTracker.Create(command.Name, command.Description, command.Type, command.Frequency, command.Goal, _currentUser.Id);
        UnitOfWork.Habits.Add(h);
        return Task.FromResult(Result.Success(h.Id));
    }
}
