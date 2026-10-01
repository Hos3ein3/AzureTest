

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.MVC.Infrastructure;

public class DashboardRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var value) && value != null)
        {
            var slug = value.ToString();
            
            var options = httpContext?.RequestServices.GetService<Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings>>();
            var reserved = options?.Value.ReservedSlugs ?? new List<string>();
            
            if (!string.IsNullOrEmpty(slug) && !reserved.Contains(slug, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
