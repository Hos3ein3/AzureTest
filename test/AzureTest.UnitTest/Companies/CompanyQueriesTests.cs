using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Companies.Queries.GetAllCompanies;
using AzureTest.Application.Companies.Queries.GetCompanyById;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Companies;
using AzureTest.Domain.Documents;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Companies;

public class CompanyQueriesTests
{
    private readonly Mock<INoSqlUnitOfWork> _mockNoSqlUnitOfWork;
    private readonly Mock<IUnitOfWork>  _mockUnitOfWork;
    private readonly Mock<ICompanyRepository> _mockCompanyRepository;
    private readonly Mock<ICompanyCacheService> _mockCacheService;

    public CompanyQueriesTests()
    {
        _mockNoSqlUnitOfWork= new Mock<INoSqlUnitOfWork>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCompanyRepository = new Mock<ICompanyRepository>();
        _mockCacheService = new Mock<ICompanyCacheService>();
    }

    [Fact]
    public async Task GetCompanyById_Should_ReturnDto_WhenCompanyExists()
    {
        // Arrange
        var handler = new GetCompanyByIdQueryHandler(_mockNoSqlUnitOfWork.Object);
        var companyId = Guid.NewGuid();
        var company = new Company 
        { 
            Id = companyId, 
            CompanyName = "Test Company", 
            KvkNumber = "123", 
            IsINDMember = true,
            Location = "Amsterdam"
        };

        _mockCompanyRepository.Setup(x => x.GetByIdAsync(companyId)).ReturnsAsync(company);
        var query = new GetCompanyByIdQuery(companyId);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(companyId);
        result.CompanyName.Should().Be("Test Company");
    }

    [Fact]
    public async Task GetCompanyById_Should_ReturnNull_WhenCompanyDoesNotExist()
    {
        // Arrange
        var handler = new GetCompanyByIdQueryHandler(_mockNoSqlUnitOfWork.Object);
        _mockCompanyRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Company?)null);
        var query = new GetCompanyByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllCompanies_Should_ReturnFromCache_WhenCacheIsNotEmpty()
    {
        // Arrange
        var handler = new GetAllCompaniesQueryHandler(_mockNoSqlUnitOfWork.Object, _mockCacheService.Object);
        var cachedDtos = new List<CompanyDto>
        {
            new CompanyDto(Guid.NewGuid(), "Cached Company 1", "111", true, "Rotterdam"),
            new CompanyDto(Guid.NewGuid(), "Cached Company 2", "222", false, "Amsterdam")
        };

        _mockCacheService.Setup(x => x.GetCompaniesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDtos);

        var query = new GetAllCompaniesQuery(new GetAllCompaniesFilters());

        // Act
        var pagedResult = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        pagedResult.Should().NotBeNull();
        pagedResult.TotalCount.Should().Be(2);
        pagedResult.Items.Should().HaveCount(2);
        _mockCompanyRepository.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllCompanies_Should_FetchFromRepository_WhenCacheIsEmpty_And_ApplyFilters()
    {
        // Arrange
        var handler = new GetAllCompaniesQueryHandler(_mockNoSqlUnitOfWork.Object, _mockCacheService.Object);
        var companies = new List<Company>
        {
            new Company { Id = Guid.NewGuid(), CompanyName = "Tech Corp", IsINDMember = true, Location = "Utrecht" },
            new Company { Id = Guid.NewGuid(), CompanyName = "Food Ltd", IsINDMember = false, Location = "Utrecht" },
            new Company { Id = Guid.NewGuid(), CompanyName = "Tech Start", IsINDMember = true, Location = "Amsterdam" }
        };

        _mockCacheService.Setup(x => x.GetCompaniesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<CompanyDto>?)null);
            
        _mockCompanyRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(companies);

        var filters = new GetAllCompaniesFilters { Name = "Tech", Location = "Utrecht", IsINDMember = true };
        var query = new GetAllCompaniesQuery(filters);

        // Act
        var pagedResult = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        pagedResult.Should().NotBeNull();
        pagedResult.TotalCount.Should().Be(1); // Only "Tech Corp" matches all filters
        pagedResult.Items.First().CompanyName.Should().Be("Tech Corp");
        
        _mockCacheService.Verify(x => x.SetCompaniesAsync(It.IsAny<IEnumerable<CompanyDto>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
