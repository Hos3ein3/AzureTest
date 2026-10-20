using System;
using System.Globalization;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Habits.Commands;

public record LogHabitCommand(
    Guid HabitId,
    DateOnly? LocalDate,
    string? Value,
    double? NumericValue,
    bool? BooleanValue,
    string? Notes
) : ICommand<Result>
{
    // Backward-compatible constructor
    public LogHabitCommand(Guid habitId, DateTime logDate, bool booleanValue, double numericValue, string? notes)
        : this(
            habitId,
            DateOnly.FromDateTime(logDate.Kind == DateTimeKind.Utc ? logDate : logDate.ToUniversalTime()),
            booleanValue ? "true" : numericValue.ToString(CultureInfo.InvariantCulture),
            numericValue,
            booleanValue,
            notes)
    {
    }
}

public sealed class LogHabitCommandValidator : AbstractValidator<LogHabitCommand>
{
    public LogHabitCommandValidator()
    {
        RuleFor(x => x.HabitId).NotEmpty().WithMessage("Habit ID is required.");
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
