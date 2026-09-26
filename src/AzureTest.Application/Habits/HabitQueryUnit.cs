using System.Collections.Generic;
using AzureTest.Application.Habits.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Habits;

namespace AzureTest.Application.Habits;

public class HabitQueryUnit : IHabitQueryUnit
{
    public IQueryHandler<GetAllHabitsQuery, IReadOnlyList<HabitTrackerDto>> GetAllHabits { get; }
    public IQueryHandler<GetHabitByIdQuery, HabitTrackerDto?> GetHabitById { get; }

    public HabitQueryUnit(
        IQueryHandler<GetAllHabitsQuery, IReadOnlyList<HabitTrackerDto>> getAllHabits,
        IQueryHandler<GetHabitByIdQuery, HabitTrackerDto?> getHabitById)
    {
        GetAllHabits = getAllHabits;
        GetHabitById = getHabitById;
    }
}
