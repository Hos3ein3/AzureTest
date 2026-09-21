using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Resumes;

public sealed record CreateResumeDto(
    string PositionTitle,
    string Description,
    string Location,
    string JobLink,
    string Note,
    ResumeSource Source,
    System.DateTime CreatedOn,
    ResumeStatus Status = ResumeStatus.Applied) : IResumeFormDto;
