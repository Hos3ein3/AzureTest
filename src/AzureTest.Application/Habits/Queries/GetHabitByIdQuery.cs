using System;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Habits;

namespace AzureTest.Application.Habits.Queries;

public record GetHabitByIdQuery(Guid Id, Guid UserId) : IQuery<HabitTrackerDto?>;
