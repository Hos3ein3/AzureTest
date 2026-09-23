using AzureTest.Contracts.DTOs.Resumes;
using FluentValidation;

namespace AzureTest.Contracts.Validators.Resumes;

public sealed class CreateResumeDtoValidator : AbstractValidator<CreateResumeDto>
{
    public CreateResumeDtoValidator()
    {
        RuleFor(x => x.PositionTitle)
            .NotEmpty().WithMessage("Position title is required.")
            .MaximumLength(200).WithMessage("Position title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(10000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters.");

        RuleFor(x => x.JobLink)
            .MaximumLength(500).WithMessage("Job link must not exceed 500 characters.")
            .Must(link => string.IsNullOrWhiteSpace(link) || Uri.TryCreate(link, UriKind.Absolute, out _))
            .WithMessage("Job link must be a valid URL.");

        RuleFor(x => x.Note)
            .MaximumLength(5000).WithMessage("Note must not exceed 1000 characters.");

        RuleFor(x => x.Source)
            .IsInEnum().WithMessage("Source must be a valid value.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid value.");
        
        RuleFor(x=>x.ChatUrl).MaximumLength(500).WithMessage("Chat url must not exceed 500 characters.")
            .Must(link => string.IsNullOrWhiteSpace(link) || Uri.TryCreate(link, UriKind.Absolute, out _))
            .WithMessage("Chat Url must be a valid URL.");
    }
}
