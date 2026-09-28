using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;

namespace AzureTest.MVC.Infrastructure;

public class DashboardRouteConstraint : IRouteConstraint
{
    // A list of existing controllers so we don't accidentally swallow normal routes.
    // E.g., /Expense/Index should go to ExpenseController, not DashboardController.
    private static readonly HashSet<string> ReservedControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Home", "Auth", "Account", "Resume", "Expense", "Debt", "Task", "Calendar", "Habit", "Ticket", "Dashboard", "Health", "Hangfire"
    };

    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var value) && value != null)
        {
            var slug = value.ToString();
            // If the requested slug is NOT a reserved controller, assume it's a dynamic dashboard route.
            if (!string.IsNullOrEmpty(slug) && !ReservedControllers.Contains(slug))
            {
                return true;
            }
        }
        return false;
    }
}
