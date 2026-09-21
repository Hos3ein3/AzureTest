using AzureTest.Contracts.DTOs.Resumes;
using FluentValidation;

namespace AzureTest.Contracts.Validators.Resumes;

public sealed class UpdateResumeStatusDtoValidator : AbstractValidator<UpdateResumeStatusDto>
{
    public UpdateResumeStatusDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Resume ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid value.");
    }
}
