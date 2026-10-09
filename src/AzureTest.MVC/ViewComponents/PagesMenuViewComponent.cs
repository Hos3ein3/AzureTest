using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Pages;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.ViewComponents;

public class PagesMenuViewComponent : ViewComponent
{
    private readonly GetAllPagesQueryHandler _getAllPagesQuery;

    public PagesMenuViewComponent(GetAllPagesQueryHandler getAllPagesQuery)
    {
        _getAllPagesQuery = getAllPagesQuery;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var filter = new PageFilter { Status = AzureTest.Domain.Enums.PageStatus.Published };
        var result = await _getAllPagesQuery.HandleAsync(new GetAllPagesQuery(filter));
        
        var pages = result.IsSuccess ? result.Value.OrderBy(p => p.Title).ToList() : new();
        return View(pages);
    }
}
