using AzureTest.Application.Abstractions;

using System;

using AzureTest.Application.Messaging;

namespace AzureTest.Application.Habits.Commands;

public record DeleteHabitCommand(Guid Id) : ICommand<Result>;
