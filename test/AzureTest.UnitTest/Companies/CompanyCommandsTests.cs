using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Companies.Commands.CreateCompany;
using AzureTest.Application.Companies.Commands.DeleteCompany;
using AzureTest.Application.Companies.Commands.UpdateCompany;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Companies;
using AzureTest.Domain.Documents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Companies;

public class CompanyCommandsTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<INoSqlUnitOfWork>  _mockNoSqlUnitOfWork;
    private readonly Mock<ICompanyRepository> _mockCompanyRepository;
    private readonly Mock<ICompanyCacheService> _mockCacheService;

    public CompanyCommandsTests()
    {
        _mockNoSqlUnitOfWork = new Mock<INoSqlUnitOfWork>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCompanyRepository = new Mock<ICompanyRepository>();
        _mockCacheService = new Mock<ICompanyCacheService>();
        _mockNoSqlUnitOfWork.Setup(u => u.Companies).Returns(_mockCompanyRepository.Object);
    }

    [Fact]
    public async Task CreateCompany_Should_ReturnCompanyId_And_InvalidateCache()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CreateCompanyCommandHandler>>();
        var handler = new CreateCompanyCommandHandler(
            _mockUnitOfWork.Object,
            _mockNoSqlUnitOfWork.Object,
            _mockCacheService.Object,
            mockLogger.Object);

        var dto = new CreateCompanyDto("Test Company", "12345678", true, "Amsterdam");
        var command = new CreateCompanyCommand(dto);

        _mockCompanyRepository.Setup(x => x.AddAsync(It.IsAny<Company>()))
            .Returns(Task.CompletedTask);
        
        _mockCacheService.Setup(x => x.InvalidateCompaniesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Value.Should().NotBeEmpty();
        _mockCompanyRepository.Verify(x => x.AddAsync(It.Is<Company>(c => c.CompanyName == "Test Company")), Times.Once);
        _mockCacheService.Verify(x => x.InvalidateCompaniesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // BaseCommandHandler calls CommitAsync
    }

    [Fact]
    public async Task UpdateCompany_Should_ReturnTrue_WhenCompanyExists()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UpdateCompanyCommandHandler>>();
        var handler = new UpdateCompanyCommandHandler(
            _mockUnitOfWork.Object,
            _mockNoSqlUnitOfWork.Object,
            _mockCacheService.Object,
            mockLogger.Object);

        var companyId = Guid.NewGuid();
        var existingCompany = new Company { Id = companyId, CompanyName = "Old Name" };
        var dto = new UpdateCompanyDto(companyId, "New Name", "87654321", false, "Rotterdam");
        var command = new UpdateCompanyCommand(dto);

        _mockCompanyRepository.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(existingCompany);
        
        _mockCompanyRepository.Setup(x => x.UpdateAsync(companyId, It.IsAny<Company>()))
            .Returns(Task.CompletedTask);
            
        _mockCacheService.Setup(x => x.InvalidateCompaniesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockCompanyRepository.Verify(x => x.UpdateAsync(companyId, It.Is<Company>(c => c.CompanyName == "New Name")), Times.Once);
        _mockCacheService.Verify(x => x.InvalidateCompaniesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCompany_Should_ReturnTrue_WhenCompanyExists()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DeleteCompanyCommandHandler>>();
        var handler = new DeleteCompanyCommandHandler(
            _mockUnitOfWork.Object,
            _mockNoSqlUnitOfWork.Object,
            _mockCacheService.Object,
            mockLogger.Object);

        var companyId = Guid.NewGuid();
        var existingCompany = new Company { Id = companyId };
        var command = new DeleteCompanyCommand(companyId);

        _mockCompanyRepository.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(existingCompany);
        
        _mockCompanyRepository.Setup(x => x.RemoveAsync(companyId))
            .Returns(Task.CompletedTask);
            
        _mockCacheService.Setup(x => x.InvalidateCompaniesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockCompanyRepository.Verify(x => x.RemoveAsync(companyId), Times.Once);
        _mockCacheService.Verify(x => x.InvalidateCompaniesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
