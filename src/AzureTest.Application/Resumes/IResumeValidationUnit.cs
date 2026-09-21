using AzureTest.Contracts.DTOs.Resumes;
using FluentValidation;

namespace AzureTest.Application.Resumes;

/// <summary>
/// Aggregates all Resume validators into a single injectable unit.
/// </summary>
public interface IResumeValidationUnit
{
    IValidator<CreateResumeDto> Create { get; }
    IValidator<UpdateResumeDto> Update { get; }
    IValidator<UpdateResumeStatusDto> UpdateStatus { get; }
}
