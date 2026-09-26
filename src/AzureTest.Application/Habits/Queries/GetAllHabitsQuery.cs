using System;
using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Habits;

namespace AzureTest.Application.Habits.Queries;

public record GetAllHabitsQuery(Guid? UserId) : IQuery<IReadOnlyList<HabitTrackerDto>>;
