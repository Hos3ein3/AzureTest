using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Habits.Commands;

public class UndoHabitLogCommandHandler : BaseCommandHandler<UndoHabitLogCommand, Result>
{
    private readonly ICurrentUser _currentUser;

    public UndoHabitLogCommandHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, ILogger<UndoHabitLogCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(UndoHabitLogCommand command, CancellationToken cancellationToken)
    {
        var habit = await UnitOfWork.Habits.GetByIdAsync(command.HabitId);
        if (habit == null || habit.UserId != _currentUser.Id) return Result.Failure("Habit not found.");

        var localDate = command.LocalDate ?? HabitStatsCalculator.GetLocalDate(DateTime.UtcNow, TimeZoneInfo.Utc);
        var latestLog = await UnitOfWork.HabitLogs.GetLatestLogAsync(command.HabitId, localDate);

        if (latestLog == null)
        {
            return Result.Failure("No log found to undo for this habit.");
        }

        latestLog.IsDeleted = true;
        latestLog.DeletedOn = DateTime.UtcNow;
        latestLog.DeletedBy = _currentUser.Id.ToString();

        return Result.Success();
    }
}
