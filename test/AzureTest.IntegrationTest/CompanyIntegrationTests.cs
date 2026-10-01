using System.Net;
using System.Threading.Tasks;
using AzureTest.Application.Companies.Commands.CreateCompany;
using AzureTest.Contracts.DTOs.Companies;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AzureTest.Application.Companies;
using AzureTest.Application.Companies.Queries.GetCompanyById;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;

namespace AzureTest.IntegrationTest;

public class CompanyIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly Faker<CreateCompanyDto> _companyFaker;

    public CompanyIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        
        // Configure Bogus faker for CreateCompanyDto
        _companyFaker = new Faker<CreateCompanyDto>()
            .CustomInstantiator(f => new CreateCompanyDto(
                f.Company.CompanyName(),
                f.Random.Replace("########"),
                f.Random.Bool(),
                f.Address.City()
            ));
    }

    [Fact]
    public async Task CreateCompany_ShouldPersistToMongoDb()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<ICompanyCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<ICompanyQueryUnit>();
        
        var newCompanyDto = _companyFaker.Generate();
        var command = new CreateCompanyCommand(newCompanyDto);

        // Act
        var companyId = (await commandUnit.CreateCompany.HandleAsync(command, default)).Value;

        // Assert
        Assert.NotEqual(System.Guid.Empty, companyId);

        var getQuery = new GetCompanyByIdQuery(companyId);
        var retrievedCompany = await queryUnit.GetById.HandleAsync(getQuery, default);

        Assert.NotNull(retrievedCompany);
        Assert.Equal(newCompanyDto.CompanyName, retrievedCompany.CompanyName);
        Assert.Equal(newCompanyDto.KvkNumber, retrievedCompany.KvkNumber);
        Assert.Equal(newCompanyDto.IsINDMember, retrievedCompany.IsINDMember);
        Assert.Equal(newCompanyDto.Location, retrievedCompany.Location);
    }
}
