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

    public static IActionResult ToView<T>(this Result<T> result, Controller controller, string? viewName = null)
        => result.ToViewResult(controller, viewName);

    public static IActionResult ToView(this Result result, Controller controller, object? model = null, string? viewName = null)
        => result.ToViewResult(controller, model, viewName);

    public static IActionResult ToForm(this Result result, Controller controller, string successAction, string? successController = null, object? model = null)
        => result.ToFormResult(controller, model ?? new object(), successAction, successController);

    public static IActionResult ToForm<T>(this Result<T> result, Controller controller, string successAction, string? successController = null, object? model = null)
        => ((Result)result).ToFormResult(controller, model ?? (object?)result.Value ?? new object(), successAction, successController);


    /// <summary>
    /// Attaches the result errors to ModelState and sets ViewData modal state flags
    /// so the UI knows which modal was active and should be reopened upon validation/operation failure.
    /// </summary>
    public static void AddToModal(this Result result, Controller controller, string modalId)
    {
        controller.ViewData["ActiveModalId"] = modalId;
        controller.ViewData["ModalSuccess"] = result.IsSuccess;
        controller.ViewData["ModalMessage"] = result.Message ?? result.Error;

        if (result.IsFailure)
        {
            result.AddToModelState(controller.ModelState);
            if (result.ValidationErrors.Count == 0 && !string.IsNullOrWhiteSpace(result.Message ?? result.Error))
            {
                controller.ModelState.AddModelError(string.Empty, result.Message ?? result.Error ?? "An error occurred.");
            }
        }
    }

    /// <summary>
    /// Modal Result Handler:
    /// - If AJAX/fetch request: returns JSON { success, modalId, message, errors }.
    /// - If standard HTTP POST:
    ///   - On Success: sets TempData toast and redirects to the specified action.
    ///   - On Failure: sets modal state flags, populates ModelState, and returns the View.
    /// </summary>
    public static IActionResult ToModal(
        this Result result,
        Controller controller,
        string modalId,
        object? model = null,
        string? successRedirectAction = null,
        string? successRedirectController = null,
        string? errorViewName = null)
    {
        bool isAjax = controller.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                   || controller.Request.Headers.Accept.ToString().Contains("application/json");

        if (isAjax)
        {
            if (result.IsSuccess)
            {
                return new JsonResult(new
                {
                    success = true,
                    modalId,
                    message = result.Message ?? "Operation completed successfully."
                });
            }

            return new JsonResult(new
            {
                success = false,
                modalId,
                message = result.Message ?? result.Error ?? "An error occurred.",
                errors = result.ValidationErrors
            });
        }

        if (result.IsSuccess)
        {
            controller.TempData.SetToast(result);
            if (!string.IsNullOrEmpty(successRedirectAction))
            {
                return successRedirectController == null
                    ? controller.RedirectToAction(successRedirectAction)
                    : controller.RedirectToAction(successRedirectAction, successRedirectController);
            }

            return controller.Ok();
        }

        result.AddToModal(controller, modalId);
        return string.IsNullOrEmpty(errorViewName)
            ? controller.View(model)
            : controller.View(errorViewName, model);
    }

    /// <summary>
    /// Generic Modal Result Handler:
    /// - If AJAX/fetch request: returns JSON { success, modalId, message, data, errors }.
    /// - If standard HTTP POST:
    ///   - On Success: sets TempData toast and redirects to the specified action.
    ///   - On Failure: sets modal state flags, populates ModelState, and returns the View.
    /// </summary>
    public static IActionResult ToModal<T>(
        this Result<T> result,
        Controller controller,
        string modalId,
        object? model = null,
        string? successRedirectAction = null,
        string? successRedirectController = null,
        string? errorViewName = null)
    {
        bool isAjax = controller.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                   || controller.Request.Headers.Accept.ToString().Contains("application/json");

        if (isAjax)
        {
            if (result.IsSuccess)
            {
                return new JsonResult(new
                {
                    success = true,
                    modalId,
                    message = result.Message ?? "Operation completed successfully.",
                    data = result.Value
                });
            }

            return new JsonResult(new
            {
                success = false,
                modalId,
                message = result.Message ?? result.Error ?? "An error occurred.",
                errors = result.ValidationErrors
            });
        }

        if (result.IsSuccess)
        {
            controller.TempData.SetToast(result);
            if (!string.IsNullOrEmpty(successRedirectAction))
            {
                return successRedirectController == null
                    ? controller.RedirectToAction(successRedirectAction)
                    : controller.RedirectToAction(successRedirectAction, successRedirectController);
            }

            return controller.Ok(result.Value);
        }

        result.AddToModal(controller, modalId);
        return string.IsNullOrEmpty(errorViewName)
            ? controller.View(model)
            : controller.View(errorViewName, model);
    }
}
