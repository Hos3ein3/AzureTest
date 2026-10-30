using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Application.UserPreferences.Commands;
using AzureTest.Application.UserPreferences.Queries;
using AzureTest.Contracts.DTOs.UserPreferences;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.UserPreference;

public static class UpdateUserPreference
{
    public static async Task<IResult> HandleAsync(
        [FromBody] UpdatePreferenceRequestDto jsonRequest,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IQueryHandler<GetUserPreferenceQuery, AzureTest.Domain.Documents.UserPreference> getUserPreferenceQuery,
        [FromServices] ICommandHandler<UpdateUserPreferenceCommand> updateUserPreferenceCommand,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var lang = jsonRequest?.Language;
        var theme = jsonRequest?.ThemeColor;
        var currency = jsonRequest?.Currency;
        var preferredDashboard = jsonRequest?.PreferredDashboard;
        var timeZone = jsonRequest?.TimeZone;
        var isDarkMode = jsonRequest?.IsDarkMode;

        var language = string.IsNullOrWhiteSpace(lang) ? "en" : lang;
        var themeColor = string.IsNullOrWhiteSpace(theme) ? "slate" : theme;
        var finalCurrency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency;
        var finalPreferredDashboard = preferredDashboard ?? string.Empty;

        var pref = await getUserPreferenceQuery.HandleAsync(new GetUserPreferenceQuery(userId), cancellationToken);
        var finalIsDarkMode = isDarkMode ?? pref.IsDarkMode;
        var finalTimeZone = timeZone ?? pref.TimeZone;

        await updateUserPreferenceCommand.HandleAsync(
            new UpdateUserPreferenceCommand(userId, language, themeColor, finalCurrency, finalPreferredDashboard, finalIsDarkMode, finalTimeZone),
            cancellationToken);

        var result = new UpdatePreferenceResultDto
        {
            Success = true,
            Language = language,
            ThemeColor = themeColor,
            Currency = finalCurrency,
            PreferredDashboard = finalPreferredDashboard,
            TimeZone = finalTimeZone,
            IsDarkMode = finalIsDarkMode
        };

        return Results.Ok(result);
    }
}
