using AzureTest.Application.Abstractions;

using System;

using AzureTest.Application.Habits.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Habits;

public class HabitCommandUnit : IHabitCommandUnit
{
    public ICommandHandler<CreateHabitCommand, Result<Guid>> CreateHabit { get; }
    public ICommandHandler<UpdateHabitCommand, Result> UpdateHabit { get; }
    public ICommandHandler<DeleteHabitCommand, Result> DeleteHabit { get; }
    public ICommandHandler<LogHabitCommand, Result> LogHabit { get; }

    public HabitCommandUnit(
        ICommandHandler<CreateHabitCommand, Result<Guid>> createHabit,
        ICommandHandler<UpdateHabitCommand, Result> updateHabit,
        ICommandHandler<DeleteHabitCommand, Result> deleteHabit,
        ICommandHandler<LogHabitCommand, Result> logHabit)
    {
        CreateHabit = createHabit;
        UpdateHabit = updateHabit;
        DeleteHabit = deleteHabit;
        LogHabit = logHabit;
    }
}
