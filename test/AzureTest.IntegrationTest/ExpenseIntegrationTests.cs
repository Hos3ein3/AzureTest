using AzureTest.Application.Expenses.Units;
using AzureTest.Application.Expenses.Commands;
using AzureTest.Application.Expenses.Queries;
using AzureTest.Contracts.DTOs.Expenses;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AzureTest.IntegrationTest;

public class ExpenseIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    
    public ExpenseIntegrationTests(IntegrationTestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateExpense_WithNewCategory_ShouldPersistToDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IExpenseCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IExpenseQueryUnit>();
        
        var dto = new CreateExpenseDto 
        {
            Amount = 50.0m,
            Date = DateTime.UtcNow,
            Description = "Coffee",
            NewCategoryName = "Food"
        };
        var command = new CreateExpenseCommand(dto);
        var id = (await commandUnit.Create.HandleAsync(command, default)).Value;
        
        var expense = await queryUnit.GetById.HandleAsync(new GetExpenseByIdQuery(id), default);
        expense.ShouldNotBeNull();
        expense.Amount.ShouldBe(50.0m);
    }

    [Fact]
    public async Task CreateExpense_ShouldBeQueryable()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IExpenseCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IExpenseQueryUnit>();
        
        var dto = new CreateExpenseDto 
        {
            Amount = 60.0m,
            Date = DateTime.UtcNow,
            Description = "Tea",
            NewCategoryName = "Food"
        };
        var id = (await commandUnit.Create.HandleAsync(new CreateExpenseCommand(dto), default)).Value;
        
        var expense = await queryUnit.GetById.HandleAsync(new GetExpenseByIdQuery(id), default);
        expense.ShouldNotBeNull();
        expense.Amount.ShouldBe(60.0m);
        expense.Description.ShouldBe("Tea");
    }

    [Fact]
    public async Task UpdateExpense_ShouldModifyFields()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IExpenseCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IExpenseQueryUnit>();
        
        var dto = new CreateExpenseDto 
        {
            Amount = 60.0m,
            Date = DateTime.UtcNow,
            Description = "Tea",
            NewCategoryName = "Food"
        };
        var id = (await commandUnit.Create.HandleAsync(new CreateExpenseCommand(dto), default)).Value;
        
        var updateDto = new UpdateExpenseDto 
        {
            Id = id,
            Amount = 70.0m,
            Date = DateTime.UtcNow,
            Description = "Green Tea",
            NewCategoryName = "Food"
        };
        await commandUnit.Update.HandleAsync(new UpdateExpenseCommand(updateDto), default);
        
        var expense = await queryUnit.GetById.HandleAsync(new GetExpenseByIdQuery(id), default);
        expense.ShouldNotBeNull();
        expense.Amount.ShouldBe(70.0m);
        expense.Description.ShouldBe("Green Tea");
    }
}
