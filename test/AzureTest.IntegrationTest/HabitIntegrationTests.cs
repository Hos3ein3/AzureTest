using AzureTest.Application.Habits;
using AzureTest.Application.Habits.Commands;
using AzureTest.Application.Habits.Queries;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AzureTest.IntegrationTest;

public class HabitIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    
    public HabitIntegrationTests(IntegrationTestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateHabit_ShouldPersistToDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IHabitCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IHabitQueryUnit>();
        
        var command = new CreateHabitCommand("Gym", "Workout", (AzureTest.Domain.Enums.HabitType)1, (AzureTest.Domain.Enums.HabitFrequency)1, 3);
        var id = (await commandUnit.CreateHabit.HandleAsync(command, default)).Value;
        
        var habit = await queryUnit.GetHabitById.HandleAsync(new GetHabitByIdQuery(id, TestUserId), default);
        habit.ShouldNotBeNull();
        habit.Name.ShouldBe("Gym");
        habit.Goal.ShouldBe(3);
    }

    [Fact]
    public async Task LogHabit_ShouldAddLogEntry()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IHabitCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IHabitQueryUnit>();
        
        var id = (await commandUnit.CreateHabit.HandleAsync(new CreateHabitCommand("Gym", "Workout", (AzureTest.Domain.Enums.HabitType)1, (AzureTest.Domain.Enums.HabitFrequency)1, 3), default)).Value;
        await commandUnit.LogHabit.HandleAsync(new LogHabitCommand(id, DateTime.UtcNow, true, 1.0, "Did it"), default);
        
        var habit = await queryUnit.GetHabitById.HandleAsync(new GetHabitByIdQuery(id, TestUserId), default);
        habit.ShouldNotBeNull();
        habit.Logs.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task DeleteHabit_ShouldRemove()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IHabitCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IHabitQueryUnit>();
        
        var id = (await commandUnit.CreateHabit.HandleAsync(new CreateHabitCommand("Gym", "Workout", (AzureTest.Domain.Enums.HabitType)1, (AzureTest.Domain.Enums.HabitFrequency)1, 3), default)).Value;
        await commandUnit.DeleteHabit.HandleAsync(new DeleteHabitCommand(id), default);
        
        var habit = await queryUnit.GetHabitById.HandleAsync(new GetHabitByIdQuery(id, TestUserId), default);
        habit.ShouldBeNull();
    }
}
