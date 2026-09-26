using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Resumes.Commands.CreateResume;
using AzureTest.Application.Resumes.Commands.DeleteResume;
using AzureTest.Application.Resumes.Commands.UpdateResume;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Enums;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Resumes;

public class ResumeCommandsTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IResumeRepository> _mockResumeRepository;
    private readonly Mock<ICompanyRepository> _mockCompanyRepository;

    public ResumeCommandsTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockResumeRepository = new Mock<IResumeRepository>();
        _mockCompanyRepository = new Mock<ICompanyRepository>();
    }

    [Fact]
    public async Task CreateResume_Should_ReturnResumeId_WhenValid()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CreateResumeCommandHandler>>();
        var handler = new CreateResumeCommandHandler(
            _mockUnitOfWork.Object,
            _mockResumeRepository.Object,
            _mockCompanyRepository.Object,
            mockLogger.Object);

        var dto = new CreateResumeDto("Software Engineer", "Description", "Amsterdam", "http://job.link", "Note", "http://chat.url", ResumeSource.Referral, DateTime.UtcNow)
        {
            NewCompanyName = "New Tech Company"
        };
        var command = new CreateResumeCommand(dto);

        _mockCompanyRepository.Setup(x => x.GetByNameAsync("New Tech Company"))
            .ReturnsAsync((Company?)null); // Company does not exist, should create it

        _mockCompanyRepository.Setup(x => x.AddAsync(It.IsAny<Company>()))
            .Returns(Task.CompletedTask);

        _mockResumeRepository.Setup(x => x.Add(It.IsAny<Resume>()));

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockCompanyRepository.Verify(x => x.AddAsync(It.Is<Company>(c => c.CompanyName == "New Tech Company")), Times.Once);
        _mockResumeRepository.Verify(x => x.Add(It.Is<Resume>(r => r.PositionTitle == "Software Engineer")), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateResume_Should_ReturnTrue_WhenResumeExists()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UpdateResumeCommandHandler>>();
        var handler = new UpdateResumeCommandHandler(
            _mockUnitOfWork.Object,
            _mockResumeRepository.Object,
            _mockCompanyRepository.Object,
            mockLogger.Object);

        var resumeId = Guid.NewGuid();
        var existingResume = Resume.Create("Old Title", "Old Desc", "Old Loc", "", "", "", null, ResumeSource.Referral, ResumeStatus.Applied);
        
        // Use reflection or private setter to set the ID if necessary, but for Moq it's often easier to just mock GetById
        var dto = new UpdateResumeDto(resumeId, "New Title", "New Desc", "New Loc", "", "", "", null, "", ResumeSource.Referral, ResumeStatus.Applied,DateTime.UtcNow)
        {
            NewCompanyName = "Existing Company"
        };
        
        var command = new UpdateResumeCommand(dto);

        _mockResumeRepository.Setup(x => x.GetByIdAsync(resumeId))
            .ReturnsAsync(existingResume);

        var existingCompany = new Company { Id = Guid.NewGuid(), CompanyName = "Existing Company" };
        _mockCompanyRepository.Setup(x => x.GetByNameAsync("Existing Company"))
            .ReturnsAsync(existingCompany);

        _mockResumeRepository.Setup(x => x.Update(It.IsAny<Resume>()));

        // Act
        var result= await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        existingResume.PositionTitle.Should().Be("New Title");
        existingResume.CompanyId.Should().Be(existingCompany.Id);
        _mockResumeRepository.Verify(x => x.Update(existingResume), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteResume_Should_ReturnTrue_WhenResumeExists()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DeleteResumeCommandHandler>>();
        var handler = new DeleteResumeCommandHandler(
            _mockUnitOfWork.Object,
            _mockResumeRepository.Object,
            mockLogger.Object);

        var resumeId = Guid.NewGuid();
        var existingResume = Resume.Create("Title", "Desc", "Loc", "", "", "", null, ResumeSource.Referral, ResumeStatus.Applied);
        var command = new DeleteResumeCommand(resumeId);

        _mockResumeRepository.Setup(x => x.GetByIdAsync(resumeId))
            .ReturnsAsync(existingResume);

        _mockResumeRepository.Setup(x => x.Update(It.IsAny<Resume>()));

        // Act
        var result=await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        existingResume.IsDeleted.Should().BeTrue();
        _mockResumeRepository.Verify(x => x.Update(existingResume), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateResumeStatus_Should_AddResumeStatusLog_WhenStatusChanges()
    {
        // Arrange
        var mockResumeStatusLogRepo = new Mock<IResumeStatusLogRepository>();
        var mockLogger = new Mock<ILogger<AzureTest.Application.Resumes.Commands.UpdateResumeStatus.UpdateResumeStatusCommandHandler>>();
        var handler = new AzureTest.Application.Resumes.Commands.UpdateResumeStatus.UpdateResumeStatusCommandHandler(
            _mockUnitOfWork.Object,
            _mockResumeRepository.Object,
            mockResumeStatusLogRepo.Object,
            mockLogger.Object);

        var resumeId = Guid.NewGuid();
        var existingResume = Resume.Create("Title", "Desc", "Loc", "", "", "", null, ResumeSource.Referral, ResumeStatus.Applied);

        _mockResumeRepository.Setup(x => x.GetByIdAsync(resumeId))
            .ReturnsAsync(existingResume);

        var dto = new UpdateResumeStatusDto(resumeId, ResumeStatus.Interview, "Passed screen");
        var command = new AzureTest.Application.Resumes.Commands.UpdateResumeStatus.UpdateResumeStatusCommand(dto);

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        existingResume.Status.Should().Be(ResumeStatus.Interview);
        mockResumeStatusLogRepo.Verify(x => x.AddAsync(It.Is<ResumeStatusLog>(l =>
            l.ResumeId == resumeId &&
            l.OldStatus == ResumeStatus.Applied &&
            l.NewStatus == ResumeStatus.Interview &&
            l.Note == "Passed screen")), Times.Once);
        _mockResumeRepository.Verify(x => x.Update(existingResume), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateResumeStatus_Should_AddResumeStatusLog_WhenStatusUnchanged_ButNoteProvided()
    {
        // Arrange
        var mockResumeStatusLogRepo = new Mock<IResumeStatusLogRepository>();
        var mockLogger = new Mock<ILogger<AzureTest.Application.Resumes.Commands.UpdateResumeStatus.UpdateResumeStatusCommandHandler>>();
        var handler = new AzureTest.Application.Resumes.Commands.UpdateResumeStatus.UpdateResumeStatusCommandHandler(
            _mockUnitOfWork.Object,
            _mockResumeRepository.Object,
            mockResumeStatusLogRepo.Object,
            mockLogger.Object);

        var resumeId = Guid.NewGuid();
        var existingResume = Resume.Create("Title", "Desc", "Loc", "", "", "", null, ResumeSource.Referral, ResumeStatus.Applied);

        _mockResumeRepository.Setup(x => x.GetByIdAsync(resumeId))
            .ReturnsAsync(existingResume);

        var dto = new UpdateResumeStatusDto(resumeId, ResumeStatus.Applied, "Followed up via email");
        var command = new AzureTest.Application.Resumes.Commands.UpdateResumeStatus.UpdateResumeStatusCommand(dto);

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        existingResume.Status.Should().Be(ResumeStatus.Applied);
        mockResumeStatusLogRepo.Verify(x => x.AddAsync(It.Is<ResumeStatusLog>(l =>
            l.ResumeId == resumeId &&
            l.OldStatus == ResumeStatus.Applied &&
            l.NewStatus == ResumeStatus.Applied &&
            l.Note == "Followed up via email")), Times.Once);
        _mockResumeRepository.Verify(x => x.Update(existingResume), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateResumeStatus_Should_NotAddResumeStatusLog_WhenStatusUnchanged_AndNoteIsEmpty()
    {
        // Arrange
        var mockResumeStatusLogRepo = new Mock<IResumeStatusLogRepository>();
        var mockLogger = new Mock<ILogger<AzureTest.Application.Resumes.Commands.UpdateResumeStatus.UpdateResumeStatusCommandHandler>>();
        var handler = new AzureTest.Application.Resumes.Commands.UpdateResumeStatus.UpdateResumeStatusCommandHandler(
            _mockUnitOfWork.Object,
            _mockResumeRepository.Object,
            mockResumeStatusLogRepo.Object,
            mockLogger.Object);

        var resumeId = Guid.NewGuid();
        var existingResume = Resume.Create("Title", "Desc", "Loc", "", "", "", null, ResumeSource.Referral, ResumeStatus.Applied);

        _mockResumeRepository.Setup(x => x.GetByIdAsync(resumeId))
            .ReturnsAsync(existingResume);

        var dto = new UpdateResumeStatusDto(resumeId, ResumeStatus.Applied, null);
        var command = new AzureTest.Application.Resumes.Commands.UpdateResumeStatus.UpdateResumeStatusCommand(dto);

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        mockResumeStatusLogRepo.Verify(x => x.AddAsync(It.IsAny<ResumeStatusLog>()), Times.Never);
        _mockResumeRepository.Verify(x => x.Update(existingResume), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
