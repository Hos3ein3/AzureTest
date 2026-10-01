using AzureTest.Application.Abstractions;

using System;

using AzureTest.Application.Messaging;

namespace AzureTest.Application.Habits.Commands;

public record LogHabitCommand(Guid HabitId, DateTime LogDate, bool BooleanValue, double NumericValue, string? Notes) : ICommand<Result>;
