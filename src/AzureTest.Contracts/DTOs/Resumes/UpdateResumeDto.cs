using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Resumes;

public sealed record UpdateResumeDto(
    Guid Id,
    string PositionTitle,
    string Description,
    string Location,
    string JobLink,
    string Note,
    string ChatUrl,
    Guid? CompanyId,
    string? NewCompanyName,
    ResumeSource Source,
    ResumeStatus Status,
    System.DateTime CreatedOn,
    int? AtsScore = null) : IResumeFormDto;
