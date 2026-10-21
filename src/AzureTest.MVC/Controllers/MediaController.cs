using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Media;
using AzureTest.Application.Media.Commands.AddItemToCustomList;
using AzureTest.Application.Media.Commands.AddWatchItem;
using AzureTest.Application.Media.Commands.ChangeWatchStatus;
using AzureTest.Application.Media.Commands.CreateCustomList;
using AzureTest.Application.Media.Commands.DeleteCustomList;
using AzureTest.Application.Media.Commands.DeleteWatchItem;
using AzureTest.Application.Media.Commands.FollowPerson;
using AzureTest.Application.Media.Commands.ImportMediaData;
using AzureTest.Application.Media.Commands.RateWatchItem;
using AzureTest.Application.Media.Commands.RemoveItemFromCustomList;
using AzureTest.Application.Media.Commands.UnfollowPerson;
using AzureTest.Application.Media.Commands.UpdateFollowedPersonNotes;
using AzureTest.Application.Media.Commands.RewatchItem;
using AzureTest.Application.Media.Commands.UpdateWatchProgress;
using AzureTest.Application.Media.Queries.ExportMediaData;
using AzureTest.Application.Media.Queries.GetCustomListById;
using AzureTest.Application.Media.Queries.GetCustomLists;
using AzureTest.Application.Media.Queries.GetDiscoverMedia;
using AzureTest.Application.Media.Queries.GetFollowedPeople;
using AzureTest.Application.Media.Queries.GetMediaCalendar;
using AzureTest.Application.Media.Queries.GetMediaDetails;
using AzureTest.Application.Media.Queries.GetMediaStats;
using AzureTest.Application.Media.Queries.GetMovieNews;
using AzureTest.Application.Media.Queries.GetPersonDetails;
using AzureTest.Application.Media.Queries.GetWatchItemById;
using AzureTest.Application.Media.Queries.GetWatchList;
using AzureTest.Application.Media.Queries.SearchMedia;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Domain.Media.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Media Hub")]
[Description("Manage personal media watchlists, discovery, actors, news, and statistics.")]
public class MediaController : Controller
{
    private readonly IValidationService _validator;
    private readonly IMediaCommandUnit _commands;
    private readonly IMediaQueryUnit _queries;
    private readonly ICurrentUser _currentUser;

    public MediaController(
        IValidationService validator,
        IMediaCommandUnit commands,
        IMediaQueryUnit queries,
        ICurrentUser currentUser)
    {
        _validator = validator;
        _commands = commands;
        _queries = queries;
        _currentUser = currentUser;
    }

    [HttpGet]
    [DisplayName("Watchlist Hub")]
    [Description("View and filter your personal watch list for movies, series, and anime.")]
    public async Task<IActionResult> Index([FromQuery] WatchListFilter filter, CancellationToken ct)
    {
        var result = await _queries.GetWatchListAsync(new GetWatchListQuery(_currentUser.Id, filter), ct);
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Discover Media")]
    [Description("Explore trending, popular, top rated, upcoming titles and anime highlights.")]
    public async Task<IActionResult> Discover(CancellationToken ct)
    {
        await PopulateUserWatchItemsAsync(ct);
        var result = await _queries.GetDiscoverMediaAsync(new GetDiscoverMediaQuery(_currentUser.Id), ct);
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Smart TMDb Recommendations")]
    [Description("Personalized movie, series, and anime recommendations based on rewatched favorites and followed talent.")]
    public async Task<IActionResult> Recommendations(CancellationToken ct)
    {
        await PopulateUserWatchItemsAsync(ct);
        var result = await _queries.GetSmartRecommendationsAsync(new AzureTest.Application.Media.Queries.GetSmartRecommendations.GetSmartRecommendationsQuery(_currentUser.Id), ct);
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Search TMDb")]
    [Description("Search for movies, TV series, anime, and actors on TMDb.")]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] string searchType = "all", CancellationToken ct = default)
    {
        ViewBag.SearchQuery = q ?? string.Empty;
        ViewBag.SearchType = searchType;

        var followedResult = await _queries.GetFollowedPeopleAsync(new GetFollowedPeopleQuery(_currentUser.Id), ct);
        var followedPersonIds = followedResult.IsSuccess && followedResult.Value != null
            ? new HashSet<int>(followedResult.Value.Select(f => f.TmdbPersonId))
            : new HashSet<int>();
        ViewBag.FollowedPersonIds = followedPersonIds;

        await PopulateUserWatchItemsAsync(ct);

        var result = await _queries.SearchMediaAsync(new SearchMediaQuery(q ?? string.Empty, searchType), ct);
        return result.ToView(this);
    }

    private async Task PopulateUserWatchItemsAsync(CancellationToken ct)
    {
        var userWatchlistResult = await _queries.GetWatchListAsync(new GetWatchListQuery(_currentUser.Id, new WatchListFilter { PageSize = 1000 }), ct);
        var userWatchItems = userWatchlistResult.IsSuccess && userWatchlistResult.Value?.Items != null
            ? userWatchlistResult.Value.Items
                .GroupBy(w => (w.TmdbId, w.Type))
                .ToDictionary(g => g.Key, g => g.First())
            : new Dictionary<(int TmdbId, MediaType Type), WatchItemDto>();
        ViewBag.UserWatchItems = userWatchItems;
    }

    [HttpGet]
    [Route("Media/Details/{id:int}/{slug?}")]
    [Route("Media/Details/{idOrSlug}")]
    [DisplayName("Title Details")]
    [Description("View synopsis, cast, trailers, streaming providers, and update your watch status.")]
    public async Task<IActionResult> Details(string? idOrSlug, int? id, string? slug, [FromQuery] MediaType? type, CancellationToken ct)
    {
        int tmdbId = id.HasValue && id.Value > 0 ? id.Value : 0;
        string? effectiveSlug = slug;
        MediaType? effectiveType = Request.Query.ContainsKey("type") ? type : null;

        if (tmdbId <= 0 && !string.IsNullOrWhiteSpace(idOrSlug))
        {
            if (int.TryParse(idOrSlug, out var parsedInt))
            {
                tmdbId = parsedInt;
            }
            else
            {
                var matchLeading = System.Text.RegularExpressions.Regex.Match(idOrSlug, @"^(\d+)[-_](.*)$");
                if (matchLeading.Success && int.TryParse(matchLeading.Groups[1].Value, out var leadingId))
                {
                    tmdbId = leadingId;
                    effectiveSlug = matchLeading.Groups[2].Value;
                }
                else
                {
                    var matchTrailing = System.Text.RegularExpressions.Regex.Match(idOrSlug, @"^(.*)[-_](\d+)$");
                    if (matchTrailing.Success && int.TryParse(matchTrailing.Groups[2].Value, out var trailingId))
                    {
                        tmdbId = trailingId;
                        effectiveSlug = matchTrailing.Groups[1].Value;
                    }
                    else
                    {
                        effectiveSlug = idOrSlug;
                        var cleanQuery = idOrSlug.Replace("-", " ").Replace("_", " ");
                        var searchResult = await _queries.SearchMediaAsync(new SearchMediaQuery(cleanQuery, "titles"), ct);
                        if (searchResult.IsSuccess && searchResult.Value != null && searchResult.Value.Count > 0)
                        {
                            var bestMatch = searchResult.Value[0];
                            tmdbId = bestMatch.Id;
                            effectiveType ??= bestMatch.Type;
                        }
                    }
                }
            }
        }

        if (tmdbId <= 0)
        {
            return NotFound("Media title not found.");
        }

        var result = await _queries.GetMediaDetailsAsync(new GetMediaDetailsQuery(tmdbId, effectiveType, _currentUser.Id, effectiveSlug), ct);
        return result.ToView(this);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("Media/SeasonEpisodes")]
    [DisplayName("Season Episodes")]
    [Description("Fetch all episodes of a TV show or anime season.")]
    public async Task<IActionResult> SeasonEpisodes(int id, int seasonNumber, CancellationToken ct)
    {
        var result = await _queries.GetSeasonEpisodesAsync(new AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQuery(id, seasonNumber), ct);
        if (result.IsFailure || result.Value == null)
        {
            return Json(Array.Empty<TvEpisodeSummaryDto>());
        }
        return Json(result.Value);
    }

    [HttpGet]
    [DisplayName("Followed People")]
    [Description("Follow actors and directors, track filmography progress, and see upcoming projects.")]
    public async Task<IActionResult> People(CancellationToken ct)
    {
        var result = await _queries.GetFollowedPeopleAsync(new GetFollowedPeopleQuery(_currentUser.Id), ct);
        return result.ToView(this);
    }

    [HttpGet]
    [Route("Media/PersonDetails/{id:int}/{slug?}")]
    [Route("Media/PersonDetails/{idOrSlug}")]
    [DisplayName("Person Filmography")]
    [Description("View full filmography for an actor or director with watch progress indicators.")]
    public async Task<IActionResult> PersonDetails(string? idOrSlug, int? id, CancellationToken ct)
    {
        int personId = id.HasValue && id.Value > 0 ? id.Value : 0;

        if (personId <= 0 && !string.IsNullOrWhiteSpace(idOrSlug))
        {
            if (int.TryParse(idOrSlug, out var parsedInt))
            {
                personId = parsedInt;
            }
            else
            {
                var matchLeading = System.Text.RegularExpressions.Regex.Match(idOrSlug, @"^(\d+)[-_]");
                if (matchLeading.Success && int.TryParse(matchLeading.Groups[1].Value, out var leadingId))
                {
                    personId = leadingId;
                }
                else
                {
                    var matchTrailing = System.Text.RegularExpressions.Regex.Match(idOrSlug, @"[-_](\d+)$");
                    if (matchTrailing.Success && int.TryParse(matchTrailing.Groups[1].Value, out var trailingId))
                    {
                        personId = trailingId;
                    }
                    else
                    {
                        var cleanQuery = idOrSlug.Replace("-", " ").Replace("_", " ");
                        var searchResult = await _queries.SearchMediaAsync(new SearchMediaQuery(cleanQuery, "people"), ct);
                        if (searchResult.IsSuccess && searchResult.Value != null && searchResult.Value.Count > 0)
                        {
                            personId = searchResult.Value[0].Id;
                        }
                    }
                }
            }
        }

        if (personId <= 0)
        {
            return NotFound("Person not found.");
        }

        var result = await _queries.GetPersonDetailsAsync(new GetPersonDetailsQuery(personId, _currentUser.Id), ct);
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Release Calendar")]
    [Description("View release dates for your wishlist and renewal updates for followed series.")]
    public async Task<IActionResult> Calendar([FromQuery] int? year, [FromQuery] int? month, CancellationToken ct = default)
    {
        var result = await _queries.GetMediaCalendarAsync(new GetMediaCalendarQuery(_currentUser.Id, year, month), ct);
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Entertainment News")]
    [Description("Read the latest movie, series, and entertainment industry news.")]
    public async Task<IActionResult> News(CancellationToken ct)
    {
        var result = await _queries.GetMovieNewsAsync(new GetMovieNewsQuery(24), ct);
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Custom Playlists")]
    [Description("Manage your custom movie and series collections.")]
    public async Task<IActionResult> Lists(CancellationToken ct)
    {
        var result = await _queries.GetCustomListsAsync(new GetCustomListsQuery(_currentUser.Id), ct);
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Playlist Details")]
    [Description("View and manage titles in a custom playlist.")]
    public async Task<IActionResult> ListDetails(Guid id, CancellationToken ct)
    {
        var result = await _queries.GetCustomListByIdAsync(new GetCustomListByIdQuery(_currentUser.Id, id), ct);
        return result.ToView(this);
    }

    [HttpGet]
    [DisplayName("Watch Statistics")]
    [Description("Analyze your watch time, favorite genres, top actors, and ratings.")]
    public async Task<IActionResult> Stats(CancellationToken ct)
    {
        var result = await _queries.GetMediaStatsAsync(new GetMediaStatsQuery(_currentUser.Id), ct);
        return result.ToView(this);
    }

    // ── Form POST Actions ──────────────────────────────────────────────────────

    private IActionResult RedirectBack(string? returnUrl, string fallbackAction, object? routeValues = null)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return routeValues != null
            ? RedirectToAction(fallbackAction, routeValues)
            : RedirectToAction(fallbackAction);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Add to Watchlist")]
    [Description("Add a title to your personal watchlist with selected status.")]
    public async Task<IActionResult> Add(AddWatchItemCommand command, string? returnUrl, CancellationToken ct)
    {
        var cmd = command with { UserId = _currentUser.Id, TenantId = _currentUser.TenantId };
        if (string.IsNullOrWhiteSpace(cmd.Title) && cmd.TmdbId > 0)
        {
            var detailsResult = await _queries.GetMediaDetailsAsync(new GetMediaDetailsQuery(cmd.TmdbId, cmd.Type, cmd.UserId), ct);
            if (detailsResult.IsSuccess && detailsResult.Value != null)
            {
                var details = detailsResult.Value;
                cmd = cmd with
                {
                    Title = details.Title,
                    OriginalTitle = cmd.OriginalTitle ?? details.OriginalTitle,
                    PosterPath = cmd.PosterPath ?? details.PosterPath,
                    BackdropPath = cmd.BackdropPath ?? details.BackdropPath,
                    Overview = cmd.Overview ?? details.Overview,
                    ReleaseDate = cmd.ReleaseDate ?? details.ReleaseDate,
                    TmdbVoteAverage = cmd.TmdbVoteAverage ?? details.VoteAverage,
                    TotalSeasons = cmd.TotalSeasons > 1 ? cmd.TotalSeasons : (details.Seasons?.Count > 0 ? details.Seasons.Count : 1),
                    TotalEpisodes = cmd.TotalEpisodes > 1 ? cmd.TotalEpisodes : (details.Seasons?.Count > 0 ? details.Seasons.Sum(s => s.EpisodeCount) : 1),
                    RuntimeMinutes = cmd.RuntimeMinutes ?? details.RuntimeMinutes,
                    Genres = cmd.Genres ?? details.Genres
                };
            }
        }

        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json");

        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            var msg = validation.Message ?? validation.Error ?? "Failed to add title to watchlist.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
            return RedirectBack(returnUrl, nameof(Index));
        }

        var result = await _commands.AddWatchItemAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Saved to your watchlist.";
            if (isAjax) return Json(new { success = true, message = "Saved to your watchlist." });
        }
        else
        {
            var msg = result.Message ?? result.Error ?? "Failed to add title to watchlist.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
        }

        return RedirectBack(returnUrl, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update Progress")]
    [Description("Update season and episode where you stopped watching.")]
    public async Task<IActionResult> UpdateProgress(UpdateWatchProgressCommand command, string? returnUrl, CancellationToken ct)
    {
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json");
        var cmd = command with { UserId = _currentUser.Id };
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            var msg = validation.Message ?? validation.Error ?? "Failed to update watch progress.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
            return RedirectBack(returnUrl, nameof(Index));
        }

        var result = await _commands.UpdateWatchProgressAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Watch progress updated.";
            if (isAjax) return Json(new { success = true, message = "Watch progress updated." });
        }
        else
        {
            var msg = result.Message ?? result.Error ?? "Failed to update watch progress.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
        }

        return RedirectBack(returnUrl, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Change Status")]
    [Description("Change watch status of a title in your watchlist.")]
    public async Task<IActionResult> ChangeStatus(ChangeWatchStatusCommand command, string? returnUrl, CancellationToken ct)
    {
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json");
        var cmd = command with { UserId = _currentUser.Id };
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            var msg = validation.Message ?? validation.Error ?? "Failed to change watch status.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
            return RedirectBack(returnUrl, nameof(Index));
        }

        var result = await _commands.ChangeWatchStatusAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Watch status updated.";
            if (isAjax) return Json(new { success = true, message = "Watch status updated." });
        }
        else
        {
            var msg = result.Message ?? result.Error ?? "Failed to change watch status.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
        }

        return RedirectBack(returnUrl, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Repeat / Rewatch Title")]
    [Description("Log another viewing of a finished movie or series to update your rewatch stats.")]
    public async Task<IActionResult> Rewatch(
        Guid watchItemId,
        bool restartWatching = false,
        RewatchTargetScope scope = RewatchTargetScope.EntireTitle,
        int? targetSeason = null,
        int? targetEpisode = null,
        string? returnUrl = null,
        CancellationToken ct = default)
    {
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json");
        var cmd = new RewatchItemCommand(_currentUser.Id, watchItemId, restartWatching, scope, targetSeason, targetEpisode);
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            var msg = validation.Message ?? validation.Error ?? "Failed to log repeat watch.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
            return RedirectBack(returnUrl, nameof(Index));
        }

        var result = await _commands.RewatchItemAsync(cmd, ct);
        if (result.IsSuccess)
        {
            var msg = restartWatching 
                ? (scope == RewatchTargetScope.Season ? $"Started rewatching Season {targetSeason}." : scope == RewatchTargetScope.Episode ? $"Started rewatching from S{targetSeason}E{targetEpisode}." : "Started rewatching from Episode 1.")
                : (scope == RewatchTargetScope.Season ? $"Logged Season {targetSeason} repeat viewing." : scope == RewatchTargetScope.Episode ? $"Logged S{targetSeason}E{targetEpisode} repeat viewing." : "Logged repeat viewing.");
            TempData["SuccessMessage"] = msg;
            if (isAjax) return Json(new { success = true, message = msg });
        }
        else
        {
            var msg = result.Message ?? result.Error ?? "Failed to log repeat watch.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
        }

        return RedirectBack(returnUrl, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Undo Rewatch")]
    [Description("Undo the latest rewatch log for a title, season, or episode.")]
    public async Task<IActionResult> UndoRewatch(
        Guid watchItemId,
        RewatchTargetScope scope = RewatchTargetScope.EntireTitle,
        int? targetSeason = null,
        int? targetEpisode = null,
        string? returnUrl = null,
        CancellationToken ct = default)
    {
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json");
        var cmd = new RewatchItemCommand(_currentUser.Id, watchItemId, Scope: scope, TargetSeason: targetSeason, TargetEpisode: targetEpisode, IsUndo: true);
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            var msg = validation.Message ?? validation.Error ?? "Failed to undo rewatch.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
            return RedirectBack(returnUrl, nameof(Index));
        }

        var result = await _commands.RewatchItemAsync(cmd, ct);
        if (result.IsSuccess)
        {
            var msg = scope == RewatchTargetScope.Season
                ? $"Undid Season {targetSeason} repeat viewing."
                : scope == RewatchTargetScope.Episode
                    ? $"Undid S{targetSeason}E{targetEpisode} repeat viewing."
                    : "Undid repeat viewing.";
            TempData["SuccessMessage"] = msg;
            if (isAjax) return Json(new { success = true, message = msg });
        }
        else
        {
            var msg = result.Message ?? result.Error ?? "Failed to undo rewatch.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
        }

        return RedirectBack(returnUrl, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Rate and Review")]
    [Description("Save rating, review, and watched date for a title.")]
    public async Task<IActionResult> Rate(RateWatchItemCommand command, string? returnUrl, CancellationToken ct)
    {
        var cmd = command with { UserId = _currentUser.Id };
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            TempData["ErrorMessage"] = validation.Message ?? validation.Error ?? "Failed to save rating.";
            return RedirectBack(returnUrl, nameof(Index));
        }

        var result = await _commands.RateWatchItemAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Rating and review saved.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error ?? "Failed to save rating.";
        }

        return RedirectBack(returnUrl, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Remove from Watchlist")]
    [Description("Remove a title from your personal watchlist.")]
    public async Task<IActionResult> Delete(DeleteWatchItemCommand command, string? returnUrl, CancellationToken ct)
    {
        var cmd = command with { UserId = _currentUser.Id };
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            TempData["ErrorMessage"] = validation.Message ?? validation.Error ?? "Failed to remove title.";
            return RedirectBack(returnUrl, nameof(Index));
        }

        var result = await _commands.DeleteWatchItemAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Removed from watchlist.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error ?? "Failed to remove title.";
        }

        return RedirectBack(returnUrl, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Toggle Follow Person")]
    [Description("Toggle follow or unfollow state for an actor or director.")]
    public async Task<IActionResult> ToggleFollowPerson(int tmdbPersonId, string? name, string? department, string? profilePath, string? returnUrl, CancellationToken ct)
    {
        var followedPeopleResult = await _queries.GetFollowedPeopleAsync(new GetFollowedPeopleQuery(_currentUser.Id), ct);
        bool isCurrentlyFollowed = followedPeopleResult.IsSuccess && followedPeopleResult.Value != null
            && System.Linq.Enumerable.Any(followedPeopleResult.Value, f => f.TmdbPersonId == tmdbPersonId);

        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json");

        if (isCurrentlyFollowed)
        {
            var unfollowResult = await _commands.UnfollowPersonAsync(new UnfollowPersonCommand(_currentUser.Id, tmdbPersonId), ct);
            if (unfollowResult.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Unfollowed {name ?? "person"}.";
                if (isAjax) return Json(new { success = true, isFollowed = false, message = $"Unfollowed {name ?? "person"}." });
            }
            else
            {
                var msg = unfollowResult.Message ?? unfollowResult.Error ?? "Failed to unfollow person.";
                TempData["ErrorMessage"] = msg;
                if (isAjax) return Json(new { success = false, message = msg });
            }
        }
        else
        {
            var cmd = new FollowPersonCommand(_currentUser.Id, _currentUser.TenantId, tmdbPersonId, name ?? "Unknown Person", department ?? "Acting", profilePath);
            var followResult = await _commands.FollowPersonAsync(cmd, ct);
            if (followResult.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Now following {name ?? "person"}.";
                if (isAjax) return Json(new { success = true, isFollowed = true, message = $"Now following {name ?? "person"}." });
            }
            else
            {
                var msg = followResult.Message ?? followResult.Error ?? "Failed to follow person.";
                TempData["ErrorMessage"] = msg;
                if (isAjax) return Json(new { success = false, message = msg });
            }
        }

        return RedirectBack(returnUrl, nameof(People));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Follow Person")]
    [Description("Follow an actor or director to track their filmography.")]
    public async Task<IActionResult> FollowPerson(FollowPersonCommand command, string? returnUrl, CancellationToken ct)
    {
        var cmd = command with { UserId = _currentUser.Id, TenantId = _currentUser.TenantId };
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            TempData["ErrorMessage"] = validation.Message ?? validation.Error ?? "Failed to follow person.";
            return RedirectBack(returnUrl, nameof(People));
        }

        var result = await _commands.FollowPersonAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Now following {cmd.Name}.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error ?? "Failed to follow person.";
        }

        return RedirectBack(returnUrl, nameof(People));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Unfollow Person")]
    [Description("Stop following an actor or director.")]
    public async Task<IActionResult> UnfollowPerson(UnfollowPersonCommand command, string? returnUrl, CancellationToken ct)
    {
        var cmd = command with { UserId = _currentUser.Id };
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            TempData["ErrorMessage"] = validation.Message ?? validation.Error ?? "Failed to unfollow person.";
            return RedirectBack(returnUrl, nameof(People));
        }

        var result = await _commands.UnfollowPersonAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Unfollowed successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error ?? "Failed to unfollow person.";
        }

        return RedirectBack(returnUrl, nameof(People));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update Person Notes")]
    [Description("Update personal notes for a followed actor or director.")]
    public async Task<IActionResult> UpdatePersonNotes(UpdateFollowedPersonNotesCommand command, string? returnUrl, CancellationToken ct)
    {
        var cmd = command with { UserId = _currentUser.Id };
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            TempData["ErrorMessage"] = validation.Message ?? validation.Error ?? "Failed to update notes.";
            return RedirectBack(returnUrl, nameof(People));
        }

        var result = await _commands.UpdateFollowedPersonNotesAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Notes updated.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error ?? "Failed to update notes.";
        }

        return RedirectBack(returnUrl, nameof(People));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create Custom List")]
    [Description("Create a new custom movie or series playlist.")]
    public async Task<IActionResult> CreateList(CreateCustomListCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = _currentUser.Id, TenantId = _currentUser.TenantId };
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            TempData["ErrorMessage"] = validation.Message ?? validation.Error ?? "Failed to create playlist.";
            return RedirectToAction(nameof(Lists));
        }

        var result = await _commands.CreateCustomListAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Playlist \"{cmd.Title}\" created.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error ?? "Failed to create playlist.";
        }

        return RedirectToAction(nameof(Lists));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Add to List")]
    [Description("Add a title to one of your custom playlists.")]
    public async Task<IActionResult> AddToList(AddItemToCustomListCommand command, string? returnUrl, CancellationToken ct)
    {
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json");
        var cmd = command with { UserId = _currentUser.Id };
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            var msg = validation.Message ?? validation.Error ?? "Failed to add title to playlist.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
            return RedirectBack(returnUrl, nameof(ListDetails), new { id = command.ListId });
        }

        var result = await _commands.AddItemToCustomListAsync(cmd, ct);
        if (result.IsSuccess)
        {
            var msg = $"Added \"{cmd.Title}\" to playlist.";
            TempData["SuccessMessage"] = msg;
            if (isAjax) return Json(new { success = true, message = msg });
        }
        else
        {
            var msg = result.Message ?? result.Error ?? "Failed to add title to playlist.";
            TempData["ErrorMessage"] = msg;
            if (isAjax) return Json(new { success = false, message = msg });
        }

        return RedirectBack(returnUrl, nameof(ListDetails), new { id = command.ListId });
    }

    [HttpGet]
    [DisplayName("User Playlists")]
    [Description("Get user custom playlists as JSON for quick addition.")]
    public async Task<IActionResult> UserLists(CancellationToken ct)
    {
        var result = await _queries.GetCustomListsAsync(new GetCustomListsQuery(_currentUser.Id), ct);
        if (result.IsSuccess && result.Value != null)
        {
            var lists = result.Value.Select(l => new { id = l.Id, title = l.Title, count = l.Items != null ? l.Items.Count : 0 });
            return Json(lists);
        }
        return Json(Array.Empty<object>());
    }

    [HttpGet]
    [Route("Media/ShowSeasonsJson")]
    [DisplayName("Show Seasons JSON")]
    [Description("Get TV show or anime seasons and episode counts.")]
    public async Task<IActionResult> ShowSeasonsJson(int id, MediaType type = MediaType.TvSeries, CancellationToken ct = default)
    {
        var result = await _queries.GetMediaDetailsAsync(new GetMediaDetailsQuery(id, type, _currentUser.Id), ct);
        if (result.IsSuccess && result.Value != null)
        {
            var seasons = (result.Value.Seasons ?? new List<TvSeasonSummaryDto>())
                .Where(s => s.SeasonNumber > 0)
                .Select(s => new {
                    seasonNumber = s.SeasonNumber,
                    name = s.Name,
                    episodeCount = s.EpisodeCount,
                    overview = s.Overview,
                    airDate = s.AirDate?.ToString("yyyy-MM-dd")
                });
            return Json(new {
                success = true,
                title = result.Value.Title,
                seasons
            });
        }
        return Json(new { success = false, message = "Could not load show seasons." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Remove from List")]
    [Description("Remove a title from a custom playlist.")]
    public async Task<IActionResult> RemoveFromList(RemoveItemFromCustomListCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = _currentUser.Id };
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            TempData["ErrorMessage"] = validation.Message ?? validation.Error ?? "Failed to remove item from playlist.";
            return RedirectToAction(nameof(ListDetails), new { id = command.ListId });
        }

        var result = await _commands.RemoveItemFromCustomListAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Removed title from playlist.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error ?? "Failed to remove item from playlist.";
        }

        return RedirectToAction(nameof(ListDetails), new { id = command.ListId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Custom List")]
    [Description("Delete an entire custom playlist.")]
    public async Task<IActionResult> DeleteList(DeleteCustomListCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = _currentUser.Id };
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
        {
            TempData["ErrorMessage"] = validation.Message ?? validation.Error ?? "Failed to delete playlist.";
            return RedirectToAction(nameof(Lists));
        }

        var result = await _commands.DeleteCustomListAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Playlist deleted.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error ?? "Failed to delete playlist.";
        }

        return RedirectToAction(nameof(Lists));
    }

    [HttpGet]
    [DisplayName("Export Media Data")]
    [Description("Download complete watch history and lists in JSON format.")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var result = await _queries.ExportMediaDataAsync(new ExportMediaDataQuery(_currentUser.Id), ct);
        if (!result.IsSuccess || result.Value == null)
        {
            return BadRequest("Could not export data.");
        }

        var json = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"media-backup-{DateTime.UtcNow:yyyyMMdd-HHmm}.json");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Import Media Data")]
    [Description("Upload and restore watch history and playlists from a JSON file.")]
    public async Task<IActionResult> Import(IFormFile? file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a valid JSON backup file.";
            return RedirectToAction(nameof(Index));
        }

        using var reader = new StreamReader(file.OpenReadStream());
        var json = await reader.ReadToEndAsync(ct);

        var cmd = new ImportMediaDataCommand(_currentUser.Id, _currentUser.TenantId, json);
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (validation.IsFailure)
            return validation.ToForm(this, nameof(Index));

        var result = await _commands.ImportMediaDataAsync(cmd, ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Import successful: {result.Value.WatchItemsImported} items, {result.Value.FollowedPeopleImported} people, {result.Value.CustomListsImported} lists.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? result.Error ?? "Failed to import media data.";
        }

        return RedirectToAction(nameof(Index));
    }
}
