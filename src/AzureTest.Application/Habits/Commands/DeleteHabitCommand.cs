using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Habits.Commands;

public record DeleteHabitCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteHabitCommandValidator : AbstractValidator<DeleteHabitCommand>
{
    public DeleteHabitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Habit ID is required.");
    }
}
