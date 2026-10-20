using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Habits.Commands;

public record UndoHabitLogCommand(Guid HabitId, DateOnly? LocalDate = null) : ICommand<Result>;

public sealed class UndoHabitLogCommandValidator : AbstractValidator<UndoHabitLogCommand>
{
    public UndoHabitLogCommandValidator()
    {
        RuleFor(x => x.HabitId).NotEmpty().WithMessage("Habit ID is required.");
    }
}
