using System;

namespace AzureTest.MVC.Filters;

/// <summary>
/// When applied to a controller or action, bypasses DynamicAuthorizationFilter,
/// allowing any authenticated user to access it.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class BypassDynamicAuthorizationAttribute : Attribute
{
}
