using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Enums;

namespace AzureTest.Application.Habits.Commands;

public record UpdateHabitCommand(Guid Id, string Name, string? Description, HabitType Type, HabitFrequency Frequency, double Goal) : ICommand<Result>;
