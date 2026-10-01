

using FluentValidation;

namespace AzureTest.Infrastructure.Configuration
{
    public class HealthCheckUISettings
    {
        public int EvaluationTimeInSeconds { get; set; }
        public int MinimumSecondsBetweenFailureNotifications { get; set; }
    }

    public class HealthCheckUISettingsValidator : AbstractValidator<HealthCheckUISettings>
    {
        public HealthCheckUISettingsValidator()
        {
            RuleFor(x => x.EvaluationTimeInSeconds).GreaterThan(0);
            RuleFor(x => x.MinimumSecondsBetweenFailureNotifications).GreaterThan(0);
        }
    }
}
