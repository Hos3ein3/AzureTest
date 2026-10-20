using System;
using System.Reflection;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureTest.MVC.Filters;

/// <summary>
/// Global MVC filter enforcing read-only behavior during administrative tenant impersonation
/// when AllowImpersonationWrites is set to false in FeatureManagement.
/// Blocks mutating requests (POST, PUT, DELETE, PATCH) across the application, while safeguarding
/// tenant switching/reset actions and authentication logouts.
/// </summary>
public class ImpersonationReadOnlyFilter : IAsyncActionFilter
{
    private readonly ITenantProvider _tenantProvider;
    private readonly InfrastructureFeatureOptions _featureOptions;
    private readonly ILogger<ImpersonationReadOnlyFilter> _logger;

    public const string ReadOnlyErrorMessage = 
        "Workspace is in Read-Only Impersonation mode. Administrative write operations (create, update, delete) are prohibited while impersonating another tenant.";

    public ImpersonationReadOnlyFilter(
        ITenantProvider tenantProvider,
        IOptions<InfrastructureFeatureOptions> featureOptions,
        ILogger<ImpersonationReadOnlyFilter> logger)
    {
        _tenantProvider = tenantProvider;
        _featureOptions = featureOptions.Value;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. If feature flag explicitly allows impersonation writes, allow through
        if (_featureOptions.AllowImpersonationWrites)
        {
            await next();
            return;
        }

        // 2. If user is not currently an Admin impersonating another tenant, allow through
        if (!_tenantProvider.IsAdminImpersonating)
        {
            await next();
            return;
        }

        // 3. Only mutating methods (POST, PUT, DELETE, PATCH) are restricted. GET/HEAD/OPTIONS are safe.
        var method = context.HttpContext.Request.Method;
        var isMutation = HttpMethods.IsPost(method) ||
                         HttpMethods.IsPut(method) ||
                         HttpMethods.IsDelete(method) ||
                         HttpMethods.IsPatch(method);

        if (!isMutation)
        {
            await next();
            return;
        }

        // 4. Check for exemption attributes and exempt controllers
        if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            // Allow actions or controllers explicitly marked with [AllowImpersonationWrite]
            if (descriptor.MethodInfo.IsDefined(typeof(AllowImpersonationWriteAttribute), inherit: true) ||
                descriptor.ControllerTypeInfo.IsDefined(typeof(AllowImpersonationWriteAttribute), inherit: true))
            {
                await next();
                return;
            }

            var controllerName = descriptor.ControllerName;

            // TenantController contains actions like SwitchTenant, ResetTenant, and TriggerBackfillJob
            // which the Admin must always be able to invoke to control or leave the impersonation session.
            if (string.Equals(controllerName, "Tenant", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            // AuthController contains Logout which the Admin must always be able to invoke
            if (string.Equals(controllerName, "Auth", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }
        }

        // 5. Block the request and log
        _logger.LogWarning(
            "Blocked mutating request {Method} {Path} by Admin {AdminId} while impersonating tenant {TenantId}. AllowImpersonationWrites is false.",
            method,
            context.HttpContext.Request.Path,
            _tenantProvider.RealUserId,
            _tenantProvider.CurrentTenantId);

        // 6. Respond based on request type: AJAX/JSON vs standard HTML Form/Redirect
        var request = context.HttpContext.Request;
        bool isAjaxOrJson = 
            request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
            request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
            (request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false);

        if (isAjaxOrJson)
        {
            context.Result = new JsonResult(new
            {
                success = false,
                isReadOnly = true,
                error = ReadOnlyErrorMessage,
                message = ReadOnlyErrorMessage
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        // Standard MVC Form POST: Set notification in TempData and redirect to ReadOnlyImpersonation page
        if (context.Controller is Controller controller)
        {
            controller.TempData["Error"] = ReadOnlyErrorMessage;
        }

        var returnUrl = "/";
        var referer = request.Headers.Referer.ToString();
        if (!string.IsNullOrWhiteSpace(referer) &&
            Uri.TryCreate(referer, UriKind.Absolute, out var refererUri) &&
            string.Equals(refererUri.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase))
        {
            returnUrl = refererUri.PathAndQuery;
        }

        context.Result = new RedirectToActionResult("ReadOnlyImpersonation", "Error", new { returnUrl });
    }
}
