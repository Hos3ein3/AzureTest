using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Resumes;

/// <summary>Read-only projection returned from queries.</summary>
public sealed record ResumeDto(
    Guid Id,
    string PositionTitle,
    string Description,
    string Location,
    string JobLink,
    string Note,
    string ChatUrl,
    ResumeSource Source,
    ResumeStatus Status,
    DateTime CreatedOn,
    DateTime? ModifiedOn,
    Guid? CompanyId = null,
    int? AtsScore = null);
