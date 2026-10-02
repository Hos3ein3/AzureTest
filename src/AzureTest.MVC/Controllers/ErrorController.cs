using AzureTest.Application.Abstractions;


using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace AzureTest.MVC.Controllers;

[Route("Error")]
[DisplayName("Error")]
[Description("Manage and control Error operations.")]
    public class ErrorController : Controller
{
    [Route("")]
    [DisplayName("Index")]
    [Description("View a list of Error.")]
    public IActionResult Index()
    {
        // Global Exception Page (500)
        return View("Error");
    }

    [Route("{statusCode}")]
    [DisplayName("Http Status Code Handler")]
    [Description("Execute Http Status Code Handler operation for Error.")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        return statusCode switch
        {
            404 => View("NotFound"),
            403 or 405 => View("NotAllowed"),
            _ => View("Error")
        };
    }
}
