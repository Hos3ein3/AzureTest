using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Dashboards.Queries;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Application.UserPreferences.Queries;
using AzureTest.Contracts.DTOs.UserPreferences;
using AzureTest.Domain.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.UserPreference;

public static class GetUserPreference
{
    public static async Task<IResult> HandleAsync(
        [FromServices] ICurrentUser currentUser,
        [FromServices] IQueryHandler<GetUserPreferenceQuery, AzureTest.Domain.Documents.UserPreference> getUserPreferenceQuery,
        [FromServices] IQueryHandler<GetUserDashboardsQuery, System.Collections.Generic.IEnumerable<UserDashboard>> getDashboardsHandler,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var pref = await getUserPreferenceQuery.HandleAsync(new GetUserPreferenceQuery(userId), cancellationToken);
        var dashboards = await getDashboardsHandler.HandleAsync(new GetUserDashboardsQuery(userId), cancellationToken);

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

        return Results.Ok(response);
    }
}
