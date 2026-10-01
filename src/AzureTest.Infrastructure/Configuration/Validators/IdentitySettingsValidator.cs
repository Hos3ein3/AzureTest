

using FluentValidation;

namespace AzureTest.Infrastructure.Configuration.Validators;

public class IdentitySettingsValidator : AbstractValidator<IdentitySettings>
{
    public IdentitySettingsValidator()
    {
        RuleFor(x => x.Password.RequiredLength).GreaterThan(0);
    }
}
