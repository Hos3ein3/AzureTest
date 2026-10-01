using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace AzureTest.Application.Abstractions;

public static class ValidationExtensions
{
    public static IDictionary<string, string[]> ToValidationErrors(this ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );
    }
}
