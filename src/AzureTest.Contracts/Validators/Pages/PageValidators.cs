using FluentValidation;
using AzureTest.Contracts.DTOs.Pages;

namespace AzureTest.Contracts.Validators.Pages;

public class CreatePageDtoValidator : AbstractValidator<CreatePageDto>
{
    public CreatePageDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
    }
}

public class UpdatePageDtoValidator : AbstractValidator<UpdatePageDto>
{
    public UpdatePageDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
