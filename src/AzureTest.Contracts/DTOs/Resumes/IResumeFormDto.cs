using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Resumes;

/// <summary>
/// Shared contract for the resume form fields used by both
/// <see cref="CreateResumeDto"/> and <see cref="UpdateResumeDto"/>.
/// Allows the _ResumeFormPartial Razor view to be strongly typed.
/// </summary>
public interface IResumeFormDto
{
    string PositionTitle { get; }
    string Description { get; }
    string Location { get; }
    string JobLink { get; }
    string Note { get; }
    ResumeSource Source { get; }
    ResumeStatus Status { get; }
    System.DateTime CreatedOn { get; }
}
