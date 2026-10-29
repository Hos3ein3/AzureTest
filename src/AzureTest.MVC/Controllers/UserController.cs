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
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Users.Queries.GetUserById;
using AzureTest.Domain.Entities.Identity;
using AzureTest.MVC.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;

    public UserController(
        IUserQueryUnit queryUnit,
        IUserCommandUnit commandUnit,
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogService)
    {
        _queryUnit = queryUnit;
        _commandUnit = commandUnit;
        _userManager = userManager;
        _auditLogService = auditLogService;
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
        var appUser = await _userManager.FindByIdAsync(id.ToString());
        if (appUser == null)
            return NotFound();

        var isLockedOut = await _userManager.IsLockedOutAsync(appUser);
        var logins = await _userManager.GetLoginsAsync(appUser);
        var roles = await _userManager.GetRolesAsync(appUser);

        // Fetch recent audit logs for this user (up to 25)
        var (logs, _) = await _auditLogService.GetLogsAsync(
            isAdmin: true,
            currentUserId: id,
            currentTenantId: Guid.Empty,
            actionType: null,
            searchTerm: null,
            pageNumber: 1,
            pageSize: 25);

        var loginProvidersList = new List<string>();
        if (!string.IsNullOrEmpty(appUser.PasswordHash))
        {
            loginProvidersList.Add("Local Password");
        }
        foreach (var l in logins)
        {
            loginProvidersList.Add(l.ProviderDisplayName ?? l.LoginProvider);
        }

        var vm = new UserEditViewModel
        {
            Id = appUser.Id,
            Email = appUser.Email ?? string.Empty,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            PhoneNumber = appUser.PhoneNumber,
            IsReadOnly = appUser.IsReadOnly,
            IsLockedOut = isLockedOut,
            LockoutEnd = appUser.LockoutEnd,
            EmailConfirmed = appUser.EmailConfirmed,
            PhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
            TwoFactorEnabled = appUser.TwoFactorEnabled,
            AccessFailedCount = appUser.AccessFailedCount,
            LoginProviders = loginProvidersList,
            Roles = roles.ToList(),
            AuditLogs = logs
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

        if (!ModelState.IsValid)
        {
            var appUser = await _userManager.FindByIdAsync(id.ToString());
            if (appUser != null)
            {
                var (logs, _) = await _auditLogService.GetLogsAsync(true, id, Guid.Empty, null, null, 1, 25);
                model.AuditLogs = logs;
                model.IsLockedOut = await _userManager.IsLockedOutAsync(appUser);
                model.LockoutEnd = appUser.LockoutEnd;
                model.EmailConfirmed = appUser.EmailConfirmed;
                model.PhoneNumberConfirmed = appUser.PhoneNumberConfirmed;
                model.TwoFactorEnabled = appUser.TwoFactorEnabled;
                model.AccessFailedCount = appUser.AccessFailedCount;
                var logins = await _userManager.GetLoginsAsync(appUser);
                var providers = new List<string>();
                if (!string.IsNullOrEmpty(appUser.PasswordHash)) providers.Add("Local Password");
                foreach (var l in logins) providers.Add(l.ProviderDisplayName ?? l.LoginProvider);
                model.LoginProviders = providers;
            }
            return View(model);
        }

        var command = new UpdateUserCommand(model.Id, model.FirstName, model.LastName, model.PhoneNumber, model.IsReadOnly);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Lock User")]
    [Description("Lock a user account until a specific UTC date/time or permanently.")]
    public async Task<IActionResult> LockUser(Guid id, string? lockDate, string? lockTime, bool isPermanent)
    {
        DateTimeOffset? lockoutEnd = null;
        if (!isPermanent && !string.IsNullOrWhiteSpace(lockDate))
        {
            var timePart = !string.IsNullOrWhiteSpace(lockTime) ? lockTime : "00:00";
            if (DateTime.TryParse($"{lockDate} {timePart}", out var parsedDt))
            {
                // Ensure kind is UTC for PostgreSQL compatibility
                var utcDt = DateTime.SpecifyKind(parsedDt, DateTimeKind.Utc);
                lockoutEnd = new DateTimeOffset(utcDt);
            }
        }

        var command = new ToggleUserLockoutCommand(id, lockoutEnd, isPermanent, Unlock: false);
        var result = await _commandUnit.ToggleUserLockout.HandleAsync(command);

        if (result.Success)
            TempData["Success"] = isPermanent ? "User has been locked permanently." : $"User locked until {lockoutEnd?.ToString("yyyy-MM-dd HH:mm")} UTC.";
        else
            TempData["Error"] = "Failed to lock user: " + string.Join(", ", result.Errors);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Unlock User")]
    [Description("Unlock a previously locked user account.")]
    public async Task<IActionResult> UnlockUser(Guid id)
    {
        var command = new ToggleUserLockoutCommand(id, Unlock: true);
        var result = await _commandUnit.ToggleUserLockout.HandleAsync(command);

        if (result.Success)
            TempData["Success"] = "User has been unlocked successfully.";
        else
            TempData["Error"] = "Failed to unlock user: " + string.Join(", ", result.Errors);

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
