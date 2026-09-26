using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Habits.Commands;

public record LogHabitCommand(Guid HabitId, DateTime LogDate, bool BooleanValue, double NumericValue, string? Notes) : ICommand<Result>;
