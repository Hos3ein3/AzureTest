using AzureTest.Contracts.DTOs.SampleNotes;
using FluentValidation;

namespace AzureTest.Contracts.Validators.SampleNotes;

public sealed class CreateSampleNoteDtoValidator : AbstractValidator<CreateSampleNoteDto>
{
    public CreateSampleNoteDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must be at most 100 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(2000).WithMessage("Content must be at most 2000 characters.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5).WithMessage("Priority must be between 1 and 5.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Please select a valid category.");
    }
}
