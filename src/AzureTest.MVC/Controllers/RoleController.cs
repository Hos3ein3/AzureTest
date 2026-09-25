using System.Threading.Tasks;
using AzureTest.Application.Users;
using AzureTest.Application.Users.Commands.CreateRole;
using AzureTest.Application.Users.Queries.GetAllRoles;
using AzureTest.MVC.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
public class RoleController : Controller
{
    private readonly IUserQueryUnit _queryUnit;
    private readonly IUserCommandUnit _commandUnit;

    public RoleController(IUserQueryUnit queryUnit, IUserCommandUnit commandUnit)
    {
        _queryUnit = queryUnit;
        _commandUnit = commandUnit;
    }

    // GET: Role
    public async Task<IActionResult> Index()
    {
        var roles = await _queryUnit.GetAllRoles.HandleAsync(new GetAllRolesQuery());
        return View(new RoleListViewModel { Roles = roles });
    }

    // GET: Role/Create
    public IActionResult Create()
    {
        return View(new RoleCreateViewModel());
    }

    // POST: Role/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var command = new CreateRoleCommand(model.RoleName);
        var result = await _commandUnit.CreateRole.HandleAsync(command);

        if (result.Success)
        {
            TempData["Success"] = "Role created successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }
}
