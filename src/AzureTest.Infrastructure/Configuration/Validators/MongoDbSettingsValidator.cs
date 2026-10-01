

using FluentValidation;

namespace AzureTest.Infrastructure.Configuration.Validators;

public class MongoDbSettingsValidator : AbstractValidator<MongoDbSettings>
{
    public MongoDbSettingsValidator()
    {
        RuleFor(x => x.DatabaseName)
            .NotEmpty()
            .Matches(@"^[^.\s]+$");
            
        RuleFor(x => x.ConnectionString)
            .NotEmpty();
    }
}
