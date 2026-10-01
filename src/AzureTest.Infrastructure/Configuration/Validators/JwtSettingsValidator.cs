

using FluentValidation;

namespace AzureTest.Infrastructure.Configuration.Validators;

public class JwtSettingsValidator : AbstractValidator<JwtSettings>
{
    public JwtSettingsValidator()
    {
        RuleFor(x => x.Issuer).NotEmpty();
        RuleFor(x => x.Audience).NotEmpty();
        RuleFor(x => x.Key).NotEmpty();
    }
}
