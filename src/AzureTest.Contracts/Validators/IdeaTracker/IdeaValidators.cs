using AzureTest.Contracts.DTOs.IdeaTracker;
using FluentValidation;

namespace AzureTest.Contracts.Validators.IdeaTracker;

public class CreateIdeaDtoValidator : AbstractValidator<CreateIdeaDto>
{
    public CreateIdeaDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Status).InclusiveBetween(0, 4);
        RuleFor(x => x.Priority).InclusiveBetween(0, 3);
    }
}

public class UpdateIdeaDtoValidator : AbstractValidator<UpdateIdeaDto>
{
    public UpdateIdeaDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Status).InclusiveBetween(0, 4);
        RuleFor(x => x.Priority).InclusiveBetween(0, 3);
    }
}
