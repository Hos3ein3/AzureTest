using System;
using System.Threading.Tasks;
using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Commands.CreateResume;
using AzureTest.Application.Resumes.Queries.GetResumeById;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Enums;

using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AzureTest.IntegrationTest;

public class ResumeIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly Faker<CreateResumeDto> _resumeFaker;

    public ResumeIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        
        // Configure Bogus faker for CreateResumeDto
        _resumeFaker = new Faker<CreateResumeDto>()
            .CustomInstantiator(f => new CreateResumeDto(
                f.Name.JobTitle(),
                f.Lorem.Paragraph(),
                f.Address.City(),
                f.Internet.Url(),
                f.Lorem.Sentence(),
                f.Internet.Url(),
                ResumeSource.Referral, // CompanyId (can be generated if we also create a company)
                DateTime.UtcNow
            ));
    }

    [Fact]
    public async Task CreateResume_ShouldPersistToPostgres()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IResumeCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IResumeQueryUnit>();
        
        var newResumeDto = _resumeFaker.Generate();
        var command = new CreateResumeCommand(newResumeDto);

        // Act
        var resumeId = (await commandUnit.Create.HandleAsync(command, default)).Value;

        // Assert
        Assert.NotEqual(Guid.Empty, resumeId);

        var getQuery = new GetResumeByIdQuery(resumeId);
        var retrievedResume = await queryUnit.GetById.HandleAsync(getQuery, default);

        Assert.NotNull(retrievedResume);
        Assert.Equal(newResumeDto.PositionTitle, retrievedResume.PositionTitle);
        Assert.Equal(newResumeDto.Description, retrievedResume.Description);
        Assert.Equal(newResumeDto.Location, retrievedResume.Location);
    }
}
