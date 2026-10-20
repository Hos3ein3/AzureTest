using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Enums;
using FluentValidation;

namespace AzureTest.Application.Habits.Commands;

public record CreateHabitCommand(
    string Name,
    string? Description,
    HabitType Type,
    HabitDirection Direction,
    double Target,
    string? Schedule,
    HabitFrequency Frequency,
    string? TimeZone,
    bool IsPrivate,
    string? TypeConfig
) : ICommand<Result<Guid>>
{
    public double Goal => Target;

    // Backward-compatible constructor
    public CreateHabitCommand(string name, string? description, HabitType type, HabitFrequency frequency, double goal)
        : this(
            name,
            description,
            type,
            HabitDirection.AtLeast,
            goal,
            frequency.ToString(),
            frequency,
            "UTC",
            false,
            null)
    {
    }
}

public sealed class CreateHabitCommandValidator : AbstractValidator<CreateHabitCommand>
{
    public CreateHabitCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Habit name is required.").MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.Target).GreaterThanOrEqualTo(0).WithMessage("Target must be non-negative.");
        RuleFor(x => x.Schedule).MaximumLength(100);
        RuleFor(x => x.TimeZone).MaximumLength(100);
    }
}
