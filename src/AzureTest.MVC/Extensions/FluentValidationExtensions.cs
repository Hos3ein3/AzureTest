using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AzureTest.MVC.Extensions;

public static class FluentValidationExtensions
{
    /// <summary>
    /// Copies FluentValidation errors into the MVC ModelStateDictionary.
    /// </summary>
    public static void AddToModelState(this ValidationResult validationResult, ModelStateDictionary modelState)
    {
        if (validationResult.IsValid)
            return;

        foreach (var error in validationResult.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
