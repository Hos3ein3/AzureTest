using AzureTest.Application.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Habits.Commands;

public class LogHabitCommandHandler : BaseCommandHandler<LogHabitCommand, Result>
{

    private readonly ICurrentUser _currentUser;

    public LogHabitCommandHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, ILogger<LogHabitCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(LogHabitCommand command, CancellationToken cancellationToken)
    {
        var h = await UnitOfWork.Habits.GetByIdAsync(command.HabitId);
        if (h == null || h.UserId != _currentUser.Id) return Result.Failure("Habit not found.");

        var existingLog = await UnitOfWork.HabitLogs.GetHabitLogByDateAsync(command.HabitId, command.LogDate.Date);
        if (existingLog != null)
        {
            existingLog.Update(command.BooleanValue, command.NumericValue, command.Notes);
        }
        else
        {
            var newLog = HabitLog.Create(command.HabitId, command.LogDate.Date, command.BooleanValue, command.NumericValue, command.Notes);
            UnitOfWork.HabitLogs.Add(newLog);
        }
        
        return Result.Success();
    }
}
