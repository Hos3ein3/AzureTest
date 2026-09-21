using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Resumes;

public sealed record UpdateResumeDto(
    Guid Id,
    string PositionTitle,
    string Description,
    string Location,
    string JobLink,
    string Note,
    ResumeSource Source,
    ResumeStatus Status,
    System.DateTime CreatedOn) : IResumeFormDto;
