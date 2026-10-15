using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Permissions;

namespace AzureTest.MVC.Services;

public class PermissionScanner : IPermissionScanner
{
    public List<DiscoveredPermissionDto> ScanPermissions()
    {
        var permissions = new List<DiscoveredPermissionDto>();
        var assembly = Assembly.GetExecutingAssembly(); // AzureTest.MVC

        var controllers = assembly.GetTypes()
            .Where(type => typeof(Controller).IsAssignableFrom(type) || typeof(ControllerBase).IsAssignableFrom(type))
            .Where(type => !type.IsAbstract);

        foreach (var controller in controllers)
        {
            var controllerName = controller.Name.Replace("Controller", "");
            var areaAttr = controller.GetCustomAttribute<AreaAttribute>();
            var areaName = areaAttr?.RouteValue ?? string.Empty;

            var controllerDisplayNameAttr = controller.GetCustomAttribute<DisplayNameAttribute>();
            var controllerDescAttr = controller.GetCustomAttribute<DescriptionAttribute>();

            var cDisplayName = controllerDisplayNameAttr?.DisplayName ?? controllerName;
            var cDesc = controllerDescAttr?.Description ?? "";

            // Get all public methods that return IActionResult or Task<IActionResult> etc.
            var actions = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && 
                            (m.ReturnType == typeof(IActionResult) || 
                             m.ReturnType == typeof(Task<IActionResult>) ||
                             m.ReturnType.Name.Contains("ActionResult")))
                .ToList();

            
            foreach (var action in actions)
            {
                var actionName = action.Name;
                var actionDisplayNameAttr = action.GetCustomAttribute<DisplayNameAttribute>();
                var actionDescAttr = action.GetCustomAttribute<DescriptionAttribute>();

                var aDisplayName = actionDisplayNameAttr?.DisplayName ?? actionName;
                var aDesc = actionDescAttr?.Description ?? $"{controllerName} {actionName}";

                if (!permissions.Any(p => string.Equals(p.AreaName ?? "", areaName, System.StringComparison.OrdinalIgnoreCase) && 
                                          string.Equals(p.ControllerName, controllerName, System.StringComparison.OrdinalIgnoreCase) && 
                                          string.Equals(p.ActionName, actionName, System.StringComparison.OrdinalIgnoreCase)))
                {
                    var prefix = !string.IsNullOrWhiteSpace(areaName) ? $"[{areaName}] " : "";
                    permissions.Add(new DiscoveredPermissionDto(
                        controllerName,
                        actionName,
                        $"{prefix}{cDisplayName} - {aDisplayName}",
                        aDesc,
                        areaName
                    ));
                }
            }

        }

        return permissions;
    }
}
