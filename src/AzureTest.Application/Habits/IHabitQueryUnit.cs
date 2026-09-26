using System.Collections.Generic;
using AzureTest.Application.Habits.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Habits;

namespace AzureTest.Application.Habits;

public interface IHabitQueryUnit
{
    IQueryHandler<GetAllHabitsQuery, IReadOnlyList<HabitTrackerDto>> GetAllHabits { get; }
    IQueryHandler<GetHabitByIdQuery, HabitTrackerDto?> GetHabitById { get; }
}
