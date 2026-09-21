using AzureTest.Contracts.DTOs.Resumes;
using FluentValidation;

namespace AzureTest.Application.Resumes;

public sealed class ResumeValidationUnit : IResumeValidationUnit
{
    public IValidator<CreateResumeDto> Create { get; }
    public IValidator<UpdateResumeDto> Update { get; }
    public IValidator<UpdateResumeStatusDto> UpdateStatus { get; }

    public ResumeValidationUnit(
        IValidator<CreateResumeDto> create,
        IValidator<UpdateResumeDto> update,
        IValidator<UpdateResumeStatusDto> updateStatus)
    {
        Create = create;
        Update = update;
        UpdateStatus = updateStatus;
    }
}
