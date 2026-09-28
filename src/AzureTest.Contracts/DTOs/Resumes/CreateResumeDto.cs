using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Resumes;

public sealed record CreateResumeDto(
    string PositionTitle,
    string Description,
    string Location,
    string JobLink,
    string? Note,
    string? ChatUrl,
    ResumeSource Source,
    System.DateTime CreatedOn,
    ResumeStatus Status = ResumeStatus.Applied,
    System.Guid? CompanyId = null,
    string? NewCompanyName = null,
    int? AtsScore = null) : IResumeFormDto;
