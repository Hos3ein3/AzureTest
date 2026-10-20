using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Enums;
using FluentValidation;

namespace AzureTest.Application.Habits.Commands;

public record UpdateHabitCommand(
    Guid Id,
    string Name,
    string? Description,
    HabitType Type,
    HabitDirection Direction,
    double? Target,
    string? Schedule,
    HabitFrequency Frequency,
    string? TimeZone,
    bool IsArchived,
    bool IsPrivate,
    string? TypeConfig
) : ICommand<Result>
{
    public double? Goal => Target;

    // Backward-compatible constructor
    public UpdateHabitCommand(Guid id, string name, string? description, HabitType type, HabitFrequency frequency, double? goal)
        : this(
            id,
            name,
            description,
            type,
            HabitDirection.AtLeast,
            goal,
            frequency.ToString(),
            frequency,
            "UTC",
            false,
            false,
            null)
    {
    }
}

public sealed class UpdateHabitCommandValidator : AbstractValidator<UpdateHabitCommand>
{
    public UpdateHabitCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Habit ID is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Habit name is required.").MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.Target).GreaterThanOrEqualTo(0).When(x => x.Target.HasValue).WithMessage("Target must be non-negative.");
        RuleFor(x => x.Schedule).MaximumLength(100);
        RuleFor(x => x.TimeZone).MaximumLength(100);
    }
}
