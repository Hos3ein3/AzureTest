using System;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Enums;

namespace AzureTest.Application.Habits.Commands;

public record CreateHabitCommand(string Name, string? Description, HabitType Type, HabitFrequency Frequency, double Goal) : ICommand<Guid>;
