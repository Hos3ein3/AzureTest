using AzureTest.Application.Debts;
using AzureTest.Application.Debts.Commands;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Debts.Queries;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AzureTest.IntegrationTest;

public class DebtIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    
    public DebtIntegrationTests(IntegrationTestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateDebt_WithNewPersonName_ShouldPersistToDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IDebtCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IDebtQueryUnit>();
        
        var command = new CreateDebtCommand(100.0m, DateTime.UtcNow, null, null, 0, null, "John Doe", "Lunch money", false);
        var id = (await commandUnit.Create.HandleAsync(command, default)).Value;
        
        var debt = await queryUnit.GetById.HandleAsync(new GetDebtByIdQuery(id), default);
        debt.ShouldNotBeNull();
        debt.Amount.ShouldBe(100.0m);
        debt.Description.ShouldBe("Lunch money");
    }

    [Fact]
    public async Task GetDebtById_ShouldReturnDebt()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IDebtCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IDebtQueryUnit>();
        
        var id = (await commandUnit.Create.HandleAsync(new CreateDebtCommand(200.0m, DateTime.UtcNow, null, null, 0, null, "Jane Doe", "Dinner money", false), default)).Value;
        
        var debt = await queryUnit.GetById.HandleAsync(new GetDebtByIdQuery(id), default);
        debt.ShouldNotBeNull();
        debt.Amount.ShouldBe(200.0m);
        debt.Description.ShouldBe("Dinner money");
    }
}
