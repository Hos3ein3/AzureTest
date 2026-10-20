using System.ComponentModel;
using System.Linq;
using AzureTest.MVC.Models;
using AzureTest.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Icon Gallery")]
[Description("Explore, search, preview, and copy custom SVG icons.")]
public class IconController : Controller
{
    [HttpGet]
    [DisplayName("Icon Gallery")]
    [Description("Search, preview, and copy custom SVG icons across productivity, finance, gym, and UI modules.")]
    public IActionResult Index([FromQuery] string? search, [FromQuery] string? category)
    {
        var icons = SvgIconCatalog.Search(search, category).ToList();
        var categories = SvgIconCatalog.GetCategories();

        var vm = new IconGalleryViewModel
        {
            Icons = icons,
            Categories = categories,
            SelectedCategory = category,
            SearchTerm = search
        };

        return View("~/Views/Icon/Index.cshtml", vm);
    }
}
