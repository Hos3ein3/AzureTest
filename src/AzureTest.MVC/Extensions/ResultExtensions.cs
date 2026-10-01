using AzureTest.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AzureTest.MVC.Extensions;

public static class ResultExtensions
{
    public static void AddToModelState(this Result result, ModelStateDictionary modelState)
    {
        if (result.IsSuccess)
            return;

        foreach (var error in result.ValidationErrors)
        {
            foreach (var message in error.Value)
            {
                modelState.AddModelError(error.Key, message);
            }
        }
    }

    /// <summary>
    /// API: Returns JSON ObjectResult based on Result state (Ok, ValidationProblem, or BadRequest).
    /// </summary>
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess) return controller.Ok();

        if (result.ValidationErrors.Count > 0)
        {
            result.AddToModelState(controller.ModelState);
            return controller.ValidationProblem(controller.ModelState);
        }

        return controller.BadRequest(new ProblemDetails
        {
            Title = result.Title ?? "Bad Request",
            Detail = result.Message ?? result.Error
        });
    }

    /// <summary>
    /// API: Returns JSON ObjectResult based on Result state (Ok, ValidationProblem, or BadRequest).
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess) return controller.Ok(result.Value);

        if (result.ValidationErrors.Count > 0)
        {
            result.AddToModelState(controller.ModelState);
            return controller.ValidationProblem(controller.ModelState);
        }

        return controller.BadRequest(new ProblemDetails
        {
            Title = result.Title ?? "Bad Request",
            Detail = result.Message ?? result.Error
        });
    }

    /// <summary>
    /// MVC Form Post: If failure, fills ModelState and returns the View with the model. 
    /// If success, redirects to the specified action.
    /// </summary>
    public static IActionResult ToFormResult(this Result result, Controller controller, object model, string successRedirectAction, string? successRedirectController = null)
    {
        if (result.IsSuccess)
        {
            return successRedirectController == null 
                ? controller.RedirectToAction(successRedirectAction) 
                : controller.RedirectToAction(successRedirectAction, successRedirectController);
        }

        if (result.ValidationErrors.Count > 0)
        {
            result.AddToModelState(controller.ModelState);
        }
        else
        {
            controller.ModelState.AddModelError(string.Empty, result.Message ?? result.Error ?? "An error occurred.");
        }

        return controller.View(model);
    }

    /// <summary>
    /// MVC View: Fills ModelState if failure, then always returns a View (used for GETs or non-redirecting POSTs).
    /// </summary>
    public static IActionResult ToViewResult(this Result result, Controller controller, object? model = null, string? viewName = null)
    {
        if (result.IsFailure)
        {
            if (result.ValidationErrors.Count > 0)
                result.AddToModelState(controller.ModelState);
            else
                controller.ModelState.AddModelError(string.Empty, result.Message ?? result.Error ?? "An error occurred.");
        }

        return viewName == null ? controller.View(model) : controller.View(viewName, model);
    }
    
    /// <summary>
    /// MVC View: Fills ModelState if failure, then always returns a View (used for GETs or non-redirecting POSTs).
    /// </summary>
    public static IActionResult ToViewResult<T>(this Result<T> result, Controller controller, string? viewName = null)
    {
        if (result.IsFailure)
        {
            if (result.ValidationErrors.Count > 0)
                result.AddToModelState(controller.ModelState);
            else
                controller.ModelState.AddModelError(string.Empty, result.Message ?? result.Error ?? "An error occurred.");
                
            return viewName == null ? controller.View() : controller.View(viewName);
        }

        return viewName == null ? controller.View(result.Value) : controller.View(viewName, result.Value);
    }
}
