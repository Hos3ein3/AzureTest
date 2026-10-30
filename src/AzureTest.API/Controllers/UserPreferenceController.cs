using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using AzureTest.Application.Dashboards.Queries;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Application.UserPreferences.Commands;
using AzureTest.Application.UserPreferences.Queries;
using AzureTest.Contracts.DTOs.UserPreferences;
using AzureTest.Domain.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/user-preference")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[DisplayName("User Preference")]
[Description("Manage and control User Preference operations.")]
public class UserPreferenceController : ControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly IQueryHandler<GetUserPreferenceQuery, UserPreference> _getUserPreferenceQuery;
    private readonly ICommandHandler<UpdateUserPreferenceCommand> _updateUserPreferenceCommand;
    private readonly IQueryHandler<GetUserDashboardsQuery, System.Collections.Generic.IEnumerable<UserDashboard>> _getDashboardsHandler;

    public UserPreferenceController(
        ICurrentUser currentUser,
        IQueryHandler<GetUserPreferenceQuery, UserPreference> getUserPreferenceQuery,
        ICommandHandler<UpdateUserPreferenceCommand> updateUserPreferenceCommand,
        IQueryHandler<GetUserDashboardsQuery, System.Collections.Generic.IEnumerable<UserDashboard>> getDashboardsHandler)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _getUserPreferenceQuery = getUserPreferenceQuery ?? throw new ArgumentNullException(nameof(getUserPreferenceQuery));
        _updateUserPreferenceCommand = updateUserPreferenceCommand ?? throw new ArgumentNullException(nameof(updateUserPreferenceCommand));
        _getDashboardsHandler = getDashboardsHandler ?? throw new ArgumentNullException(nameof(getDashboardsHandler));
    }

    [HttpGet]
    [DisplayName("Get")]
    [Description("Execute Get operation for User Preference.")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, message = "User is not authenticated." });

        var pref = await _getUserPreferenceQuery.HandleAsync(new GetUserPreferenceQuery(userId), cancellationToken);
        var dashboards = await _getDashboardsHandler.HandleAsync(new GetUserDashboardsQuery(userId), cancellationToken);

        var response = new UserPreferenceResponseDto
        {
            UserId = pref.UserId,
            Language = string.IsNullOrWhiteSpace(pref.Language) ? "en" : pref.Language,
            ThemeColor = string.IsNullOrWhiteSpace(pref.ThemeColor) ? "slate" : pref.ThemeColor,
            Currency = string.IsNullOrWhiteSpace(pref.Currency) ? "USD" : pref.Currency,
            PreferredDashboard = pref.PreferredDashboard ?? string.Empty,
            IsDarkMode = pref.IsDarkMode,
            Dashboards = dashboards.Select(d => new UserPreferenceDashboardDto
            {
                Slug = d.Slug,
                Name = d.Name
            })
        };

        return Ok(response);
    }

    [HttpPost]
    [DisplayName("Update")]
    [Description("Modify an existing User Preference.")]
    public async Task<IActionResult> Update([FromBody] UpdatePreferenceRequestDto? jsonRequest, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, message = "User is not authenticated." });

        string? lang = jsonRequest?.Language;
        string? theme = jsonRequest?.ThemeColor;
        string? currency = jsonRequest?.Currency;
        string? preferredDashboard = jsonRequest?.PreferredDashboard;
        bool? isDarkMode = jsonRequest?.IsDarkMode;

        if (string.IsNullOrWhiteSpace(lang) && Request.HasFormContentType && Request.Form.ContainsKey("language"))
        {
            lang = Request.Form["language"];
        }
        if (string.IsNullOrWhiteSpace(theme) && Request.HasFormContentType && Request.Form.ContainsKey("themeColor"))
        {
            theme = Request.Form["themeColor"];
        }
        if (string.IsNullOrWhiteSpace(currency) && Request.HasFormContentType && Request.Form.ContainsKey("currency"))
        {
            currency = Request.Form["currency"];
        }
        if (string.IsNullOrWhiteSpace(preferredDashboard) && Request.HasFormContentType && Request.Form.ContainsKey("preferredDashboard"))
        {
            preferredDashboard = Request.Form["preferredDashboard"];
        }

        var language = string.IsNullOrWhiteSpace(lang) ? "en" : lang;
        var themeColor = string.IsNullOrWhiteSpace(theme) ? "slate" : theme;
        var finalCurrency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency;
        var finalPreferredDashboard = preferredDashboard ?? string.Empty;

        var pref = await _getUserPreferenceQuery.HandleAsync(new GetUserPreferenceQuery(userId), cancellationToken);
        var finalIsDarkMode = isDarkMode ?? pref.IsDarkMode;

        await _updateUserPreferenceCommand.HandleAsync(
            new UpdateUserPreferenceCommand(userId, language, themeColor, finalCurrency, finalPreferredDashboard, finalIsDarkMode),
            cancellationToken);

        var result = new UpdatePreferenceResultDto
        {
            Success = true,
            Language = language,
            ThemeColor = themeColor,
            Currency = finalCurrency,
            PreferredDashboard = finalPreferredDashboard,
            IsDarkMode = finalIsDarkMode
        };

        return Ok(result);
    }
}
