using AzureTest.Application.Abstractions;


using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Route("Error")]
public class ErrorController : Controller
{
    [Route("")]
    public IActionResult Index()
    {
        // Global Exception Page (500)
        return View("Error");
    }

    [Route("{statusCode}")]
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
