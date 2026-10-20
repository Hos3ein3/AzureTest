using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Habits.Commands;

public class CreateHabitCommandHandler : BaseCommandHandler<CreateHabitCommand, Result<Guid>>
{
    private readonly ICurrentUser _currentUser;

    public CreateHabitCommandHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, ILogger<CreateHabitCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override Task<Result<Guid>> ExecuteAsync(CreateHabitCommand command, CancellationToken cancellationToken)
    {
        var h = HabitTracker.Create(
            name: command.Name,
            description: command.Description,
            type: command.Type,
            direction: command.Direction,
            target: command.Target,
            schedule: command.Schedule,
            frequency: command.Frequency,
            timeZone: command.TimeZone,
            isPrivate: command.IsPrivate,
            typeConfig: command.TypeConfig,
            userId: _currentUser.Id
        );

        UnitOfWork.Habits.Add(h);
        return Task.FromResult(Result.Success(h.Id));
    }
}
