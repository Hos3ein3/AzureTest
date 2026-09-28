using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Application.UserPreferences.Commands;
using AzureTest.Application.UserPreferences.Queries;
using AzureTest.Domain.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class UserPreferenceController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly IQueryHandler<GetUserPreferenceQuery, UserPreference> _getUserPreferenceQuery;
    private readonly ICommandHandler<UpdateUserPreferenceCommand> _updateUserPreferenceCommand;

    public UserPreferenceController(
        ICurrentUser currentUser,
        IQueryHandler<GetUserPreferenceQuery, UserPreference> getUserPreferenceQuery,
        ICommandHandler<UpdateUserPreferenceCommand> updateUserPreferenceCommand)
    {
        _currentUser = currentUser;
        _getUserPreferenceQuery = getUserPreferenceQuery;
        _updateUserPreferenceCommand = updateUserPreferenceCommand;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var pref = await _getUserPreferenceQuery.HandleAsync(new GetUserPreferenceQuery(_currentUser.Id), cancellationToken);
        return Json(new
        {
            userId = pref.UserId,
            language = string.IsNullOrWhiteSpace(pref.Language) ? "en" : pref.Language,
            themeColor = string.IsNullOrWhiteSpace(pref.ThemeColor) ? "slate" : pref.ThemeColor
        });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Update([FromBody] UpdatePreferenceRequest? jsonRequest, CancellationToken cancellationToken)
    {
        string? lang = jsonRequest?.Language;
        string? theme = jsonRequest?.ThemeColor;

        if (string.IsNullOrWhiteSpace(lang) && Request.HasFormContentType && Request.Form.ContainsKey("language"))
        {
            lang = Request.Form["language"];
        }
        if (string.IsNullOrWhiteSpace(theme) && Request.HasFormContentType && Request.Form.ContainsKey("themeColor"))
        {
            theme = Request.Form["themeColor"];
        }

        var language = string.IsNullOrWhiteSpace(lang) ? "en" : lang;
        var themeColor = string.IsNullOrWhiteSpace(theme) ? "slate" : theme;

        await _updateUserPreferenceCommand.HandleAsync(
            new UpdateUserPreferenceCommand(_currentUser.Id, language, themeColor),
            cancellationToken);

        return Json(new { success = true, language, themeColor });
    }

    public class UpdatePreferenceRequest
    {
        public string? Language { get; set; }
        public string? ThemeColor { get; set; }
    }
}
