using AzureTest.Application.Abstractions;


using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Application.UserPreferences.Commands;
using AzureTest.Application.UserPreferences.Queries;
using AzureTest.Application.Dashboards.Queries;
using AzureTest.Domain.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("User Preference")]
[Description("Manage and control User Preference operations.")]
    public class UserPreferenceController : Controller
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
        _currentUser = currentUser;
        _getUserPreferenceQuery = getUserPreferenceQuery;
        _updateUserPreferenceCommand = updateUserPreferenceCommand;
        _getDashboardsHandler = getDashboardsHandler;
    }

    [HttpGet]
    [DisplayName("Get")]
    [Description("Execute Get operation for User Preference.")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var pref = await _getUserPreferenceQuery.HandleAsync(new GetUserPreferenceQuery(_currentUser.Id), cancellationToken);
        var dashboards = await _getDashboardsHandler.HandleAsync(new GetUserDashboardsQuery(_currentUser.Id), cancellationToken);
        
        return Json(new
        {
            userId = pref.UserId,
            language = string.IsNullOrWhiteSpace(pref.Language) ? "en" : pref.Language,
            themeColor = string.IsNullOrWhiteSpace(pref.ThemeColor) ? "slate" : pref.ThemeColor,
            currency = string.IsNullOrWhiteSpace(pref.Currency) ? "USD" : pref.Currency,
            preferredDashboard = pref.PreferredDashboard ?? string.Empty,
            dashboards = dashboards.Select(d => new { slug = d.Slug, name = d.Name })
        });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    [DisplayName("Update")]
    [Description("Modify an existing User Preference.")]
    public async Task<IActionResult> Update([FromBody] UpdatePreferenceRequest? jsonRequest, CancellationToken cancellationToken)
    {
        string? lang = jsonRequest?.Language;
        string? theme = jsonRequest?.ThemeColor;

        string? currency = jsonRequest?.Currency;
        string? preferredDashboard = jsonRequest?.PreferredDashboard;

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

        await _updateUserPreferenceCommand.HandleAsync(
            new UpdateUserPreferenceCommand(_currentUser.Id, language, themeColor, finalCurrency, finalPreferredDashboard),
            cancellationToken);

        return Json(new { success = true, language, themeColor, currency = finalCurrency, preferredDashboard = finalPreferredDashboard });
    }

    public class UpdatePreferenceRequest
    {
        public string? Language { get; set; }
        public string? ThemeColor { get; set; }
        public string? Currency { get; set; }
        public string? PreferredDashboard { get; set; }
    }
}
