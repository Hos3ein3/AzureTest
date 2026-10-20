using AzureTest.Application.Abstractions;


using System;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Users;
using AzureTest.Application.Users.Commands.AssignRolesToUser;
using AzureTest.Application.Users.Commands.CreateUser;
using AzureTest.Application.Users.Commands.DeleteUser;
using AzureTest.Application.Users.Commands.ToggleUserLockout;
using AzureTest.Application.Users.Commands.UpdateUser;
using AzureTest.Application.Users.Queries.GetAllRoles;
using AzureTest.Application.Users.Queries.GetAllUsers;
using AzureTest.Application.Users.Queries.GetUserById;
using AzureTest.MVC.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
[DisplayName("User")]
[Description("Manage and control User operations.")]
    public class UserController : Controller
{
    private readonly IUserQueryUnit _queryUnit;
    private readonly IUserCommandUnit _commandUnit;

    public UserController(IUserQueryUnit queryUnit, IUserCommandUnit commandUnit)
    {
        _queryUnit = queryUnit;
        _commandUnit = commandUnit;
    }

    // GET: User
    [DisplayName("Index")]
    [Description("View a list of User.")]
    public async Task<IActionResult> Index([FromQuery] AzureTest.Contracts.DTOs.Users.GetAllUsersFilters? filters)
    {
        filters ??= new AzureTest.Contracts.DTOs.Users.GetAllUsersFilters();

        var query = new GetAllUsersQuery(filters);
        var result = await _queryUnit.GetAllUsers.HandleAsync(query);

        var roles = await _queryUnit.GetAllRoles.HandleAsync(new GetAllRolesQuery());
        
        var vm = new UserListViewModel
        {
            Users = result,
            SearchTerm = filters.SearchTerm,
            Role = filters.Role,
            SortBy = filters.SortBy,
            SortDescending = filters.SortDescending,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize,
            AvailableRoles = roles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = r.Name,
                Text = r.Name,
                Selected = r.Name == filters.Role
            }).ToList()
        };

        ViewBag.Filters = filters;
        return View(vm);
    }
    [DisplayName("Create")]
    [Description("Create a new User.")]
    public async Task<IActionResult> Create()
    {
        var roles = await _queryUnit.GetAllRoles.HandleAsync(new GetAllRolesQuery());
        var vm = new UserCreateViewModel
        {
            AvailableRoles = roles.Select(r => new SelectListItem(r.Name, r.Name)).ToList()
        };
        return View(vm);
    }

    // POST: User/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new User.")]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var roles = await _queryUnit.GetAllRoles.HandleAsync(new GetAllRolesQuery());
            model.AvailableRoles = roles.Select(r => new SelectListItem(r.Name, r.Name)).ToList();
            return View(model);
        }

        var command = new CreateUserCommand(model.Email, model.FirstName, model.LastName, model.Password, model.SelectedRoles);
        var result = await _commandUnit.CreateUser.HandleAsync(command);

        if (result.Success)
        {
            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        var reloadRoles = await _queryUnit.GetAllRoles.HandleAsync(new GetAllRolesQuery());
        model.AvailableRoles = reloadRoles.Select(r => new SelectListItem(r.Name, r.Name)).ToList();
        return View(model);
    }

    // GET: User/Edit/5
    [DisplayName("Edit")]
    [Description("Modify an existing User.")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _queryUnit.GetUserById.HandleAsync(new GetUserByIdQuery(id));
        if (user == null)
            return NotFound();

        var vm = new UserEditViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        };

        return View(vm);
    }

    // POST: User/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Modify an existing User.")]
    public async Task<IActionResult> Edit(Guid id, UserEditViewModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid) return View(model);

        var command = new UpdateUserCommand(model.Id, model.FirstName, model.LastName, model.PhoneNumber);
        var result = await _commandUnit.UpdateUser.HandleAsync(command);

        if (result.Success)
        {
            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error);

        return View(model);
    }

    // GET: User/ManageRoles/5
    [DisplayName("Manage Roles")]
    [Description("Execute Manage Roles operation for User.")]
    public async Task<IActionResult> ManageRoles(Guid id)
    {
        var user = await _queryUnit.GetUserById.HandleAsync(new GetUserByIdQuery(id));
        if (user == null) return NotFound();

        var roles = await _queryUnit.GetAllRoles.HandleAsync(new GetAllRolesQuery());
        
        var vm = new UserRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            SelectedRoles = string.IsNullOrWhiteSpace(user.Roles) ? Array.Empty<string>() : user.Roles.Split(", ").ToArray(),
            AvailableRoles = roles.Select(r => new SelectListItem(r.Name, r.Name)).ToList()
        };

        return View(vm);
    }

    // POST: User/ManageRoles/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Manage Roles")]
    [Description("Execute Manage Roles operation for User.")]
    public async Task<IActionResult> ManageRoles(Guid userId, UserRolesViewModel model)
    {
        if (userId != model.UserId) return BadRequest();

        var command = new AssignRolesToUserCommand(model.UserId, model.SelectedRoles);
        var result = await _commandUnit.AssignRolesToUser.HandleAsync(command);

        if (result.Success)
        {
            TempData["Success"] = "User roles updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error);

        var roles = await _queryUnit.GetAllRoles.HandleAsync(new GetAllRolesQuery());
        model.AvailableRoles = roles.Select(r => new SelectListItem(r.Name, r.Name)).ToList();
        return View(model);
    }

    // POST: User/ToggleLockout/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Toggle Lockout")]
    [Description("Execute Toggle Lockout operation for User.")]
    public async Task<IActionResult> ToggleLockout(Guid id)
    {
        var command = new ToggleUserLockoutCommand(id);
        var result = await _commandUnit.ToggleUserLockout.HandleAsync(command);

        if (result.Success)
            TempData["Success"] = "User lockout status toggled successfully.";
        else
            TempData["Error"] = "Failed to toggle user lockout: " + string.Join(", ", result.Errors);

        return RedirectToAction(nameof(Index));
    }

    // POST: User/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a User.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteUserCommand(id);
        var result = await _commandUnit.DeleteUser.HandleAsync(command);

        if (result.Success)
            TempData["Success"] = "User deleted successfully.";
        else
            TempData["Error"] = "Failed to delete user: " + string.Join(", ", result.Errors);

        return RedirectToAction(nameof(Index));
    }
}
