using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Infrastructure.Services;

public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result> ValidateAsync<T>(T model, CancellationToken cancellationToken = default)
    {
        // Resolve all registered validators for this type
        var validators = _serviceProvider.GetServices<IValidator<T>>();
        
        if (!validators.Any())
        {
            return Result.Success();
        }

        var context = new ValidationContext<T>(model);
        var errors = new Dictionary<string, List<string>>();

        foreach (var validator in validators)
        {
            var validationResult = await validator.ValidateAsync(context, cancellationToken);
            
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    if (!errors.ContainsKey(error.PropertyName))
                    {
                        errors[error.PropertyName] = new List<string>();
                    }
                    errors[error.PropertyName].Add(error.ErrorMessage);
                }
            }
        }

        if (errors.Any())
        {
            var formattedErrors = errors.ToDictionary(k => k.Key, v => v.Value.ToArray());
            return Result.ValidationFailure(formattedErrors);
        }

        return Result.Success();
    }
}
