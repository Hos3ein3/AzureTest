using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("UI Components Showcase")]
[Description("Explore interactive samples, live usage previews, and copyable code for all project UI components.")]
public class UiComponentsController : Controller
{
    [HttpGet]
    [DisplayName("UI Components Catalog")]
    [Description("Live interactive preview and copy-paste code snippets for buttons, inputs, dropdowns, modals, toasts, pagination, and filters.")]
    public IActionResult Index()
    {
        return View();
    }
}
