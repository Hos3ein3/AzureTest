using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Infrastructure.Configuration;

public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext == null) return false;

        // Admin role required
        if (httpContext.User?.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin"))
        {
            return true;
        }

        // Allow localhost in Development mode
        var env = httpContext.RequestServices.GetService<IWebHostEnvironment>();
        if (env != null && env.EnvironmentName == "Development")
        {
            var ip = httpContext.Connection.RemoteIpAddress;
            if (ip != null && (ip.ToString() == "127.0.0.1" || ip.ToString() == "::1" || ip.Equals(httpContext.Connection.LocalIpAddress)))
            {
                return true;
            }
        }

        return false;
    }
}
