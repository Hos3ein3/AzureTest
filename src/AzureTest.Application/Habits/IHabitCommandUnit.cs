using AzureTest.Application.Abstractions;

using System;

using AzureTest.Application.Habits.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Habits;

public interface IHabitCommandUnit
{
    ICommandHandler<CreateHabitCommand, Result<Guid>> CreateHabit { get; }
    ICommandHandler<UpdateHabitCommand, Result> UpdateHabit { get; }
    ICommandHandler<DeleteHabitCommand, Result> DeleteHabit { get; }
    ICommandHandler<LogHabitCommand, Result> LogHabit { get; }
}
