using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Habits.Commands;

public class DeleteHabitCommandHandler : BaseCommandHandler<DeleteHabitCommand, Result>
{

    private readonly ICurrentUser _currentUser;

    public DeleteHabitCommandHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, ILogger<DeleteHabitCommandHandler> logger) : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(DeleteHabitCommand command, CancellationToken cancellationToken)
    {
        var h = await UnitOfWork.Habits.GetByIdAsync(command.Id);
        if (h == null || h.UserId != _currentUser.Id) return Result.Failure("Habit not found.");

        h.IsDeleted = true;
        h.DeletedOn = DateTime.UtcNow;
        h.DeletedBy = _currentUser.Id.ToString();
        
        return Result.Success();
    }
}
