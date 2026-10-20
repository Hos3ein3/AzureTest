using System;

namespace AzureTest.MVC.Filters;

/// <summary>
/// When applied to an MVC controller or action method, allows mutating HTTP requests (POST, PUT, DELETE, PATCH)
/// to execute even while an Administrator is in Read-Only Impersonation mode.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AllowImpersonationWriteAttribute : Attribute
{
}
