using AzureTest.Application.Tasks;
using AzureTest.Application.Tasks.Commands;
using AzureTest.Application.Tasks.Queries;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AzureTest.IntegrationTest;

public class TaskIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    
    public TaskIntegrationTests(IntegrationTestWebAppFactory factory) => _factory = factory;
    
    [Fact]
    public async Task CreateTask_ShouldPersistToDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<ITaskCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<ITaskQueryUnit>();
        
        var command = new CreateTaskCommand("Test Task", "Description", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "High");
        var result = await commandUnit.CreateTask.HandleAsync(command, default);
        var id = result.Value;
        
        var query = new GetTaskByIdQuery(id, TestUserId);
        var task = await queryUnit.GetTaskById.HandleAsync(query, default);
        
        task.ShouldNotBeNull();
        task.Title.ShouldBe("Test Task");
        task.Description.ShouldBe("Description");
    }

    [Fact]
    public async Task UpdateTask_ShouldModifyFields()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<ITaskCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<ITaskQueryUnit>();
        
        var createCmd = new CreateTaskCommand("Old Title", "Old Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "High");
        var id = (await commandUnit.CreateTask.HandleAsync(createCmd, default)).Value;
        
        var updateCmd = new UpdateTaskCommand(id, "New Title", "New Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(2), "Low");
        await commandUnit.UpdateTask.HandleAsync(updateCmd, default);
        
        var task = await queryUnit.GetTaskById.HandleAsync(new GetTaskByIdQuery(id, TestUserId), default);
        task.ShouldNotBeNull();
        task.Title.ShouldBe("New Title");
        task.Description.ShouldBe("New Desc");
    }

    [Fact]
    public async Task ToggleTask_ShouldFlipCompletion()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<ITaskCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<ITaskQueryUnit>();
        
        var createCmd = new CreateTaskCommand("Toggle Task", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "High");
        var id = (await commandUnit.CreateTask.HandleAsync(createCmd, default)).Value;
        
        await commandUnit.ToggleTask.HandleAsync(new ToggleTaskCommand(id), default);
        var task1 = await queryUnit.GetTaskById.HandleAsync(new GetTaskByIdQuery(id, TestUserId), default);
        task1.IsCompleted.ShouldBeTrue();
        
        await commandUnit.ToggleTask.HandleAsync(new ToggleTaskCommand(id), default);
        var task2 = await queryUnit.GetTaskById.HandleAsync(new GetTaskByIdQuery(id, TestUserId), default);
        task2.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteTask_ShouldRemoveFromDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<ITaskCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<ITaskQueryUnit>();
        
        var createCmd = new CreateTaskCommand("Delete Task", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "High");
        var id = (await commandUnit.CreateTask.HandleAsync(createCmd, default)).Value;
        
        await commandUnit.DeleteTask.HandleAsync(new DeleteTaskCommand(id), default);
        
        var task = await queryUnit.GetTaskById.HandleAsync(new GetTaskByIdQuery(id, TestUserId), default);
        task.ShouldBeNull();
    }
}
