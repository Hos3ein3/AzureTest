using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Resumes.Queries.GetAllResumes;
using AzureTest.Application.Resumes.Queries.GetResumeById;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Enums;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Resumes;

public class ResumeQueriesTests
{
    private readonly Mock<IResumeRepository> _mockResumeRepository;

    public ResumeQueriesTests()
    {
        _mockResumeRepository = new Mock<IResumeRepository>();
    }

    [Fact]
    public async Task GetResumeById_Should_ReturnDto_WhenResumeExists()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GetResumeByIdQueryHandler>>();
        var handler = new GetResumeByIdQueryHandler(_mockResumeRepository.Object, mockLogger.Object);
        var resumeId = Guid.NewGuid();
        var resume = Resume.Create("Software Engineer", "Desc", "Loc", "", "", "", null, ResumeSource.Referral, ResumeStatus.Applied);
        // We'll mock GetByIdAsync
        
        _mockResumeRepository.Setup(x => x.GetResumeWithDetailsAsync(resumeId))
            .ReturnsAsync(resume);
            
        var query = new GetResumeByIdQuery(resumeId);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PositionTitle.Should().Be("Software Engineer");
    }

    [Fact]
    public async Task GetResumeById_Should_ReturnNull_WhenResumeDoesNotExist()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GetResumeByIdQueryHandler>>();
        var handler = new GetResumeByIdQueryHandler(_mockResumeRepository.Object, mockLogger.Object);
        
        _mockResumeRepository.Setup(x => x.GetResumeWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Resume?)null);
            
        var query = new GetResumeByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllResumes_Should_ApplyFilters_And_ReturnPagedResult()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GetAllResumesQueryHandler>>();
        var handler = new GetAllResumesQueryHandler(_mockResumeRepository.Object, mockLogger.Object);
        
        var resumes = new List<Resume>
        {
            Resume.Create("Software Engineer", "C# Developer", "Amsterdam", "", "", "", null, ResumeSource.Referral, ResumeStatus.Applied),
            Resume.Create("Data Scientist", "Python", "Rotterdam", "", "", "", null, ResumeSource.Referral, ResumeStatus.Applied),
            Resume.Create("Frontend Engineer", "React", "Amsterdam", "", "", "", null, ResumeSource.Referral, ResumeStatus.Applied)
        };

        _mockResumeRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Resume, bool>>>()))
            .ReturnsAsync(resumes);

        var filters = new GetAllResumesFilters { Location = "Amsterdam" };
        var query = new GetAllResumesQuery(filters);

        // Act
        var pagedResult = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        pagedResult.Should().NotBeNull();
        pagedResult.TotalCount.Should().Be(2); // Two in Amsterdam
        pagedResult.Items.Should().HaveCount(2);
        pagedResult.Items.Should().Contain(x => x.PositionTitle == "Software Engineer");
        pagedResult.Items.Should().Contain(x => x.PositionTitle == "Frontend Engineer");
    }
}
