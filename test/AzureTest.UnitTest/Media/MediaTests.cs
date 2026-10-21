using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Media.Commands.AddWatchItem;
using AzureTest.Application.Media.Commands.CreateCustomList;
using AzureTest.Application.Media.Commands.RateWatchItem;
using AzureTest.Application.Media.Commands.UpdateWatchProgress;
using AzureTest.Domain.Documents.Media;
using AzureTest.Domain.Media.Enums;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace AzureTest.UnitTest.Media;

public class MediaTests
{
    [Fact]
    public void WatchItemDocument_Create_SetsPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var item = WatchItemDocument.Create(
            userId: userId,
            tenantId: tenantId,
            tmdbId: 157336, // Interstellar
            type: MediaType.Movie,
            title: "Interstellar",
            originalTitle: "Interstellar",
            posterPath: "/gEU2QniE6EwfVDxCzsAnbtDzPnh.jpg",
            backdropPath: "/xJHokMbljvjADYdit5fK5VQsXEG.jpg",
            overview: "A team of explorers travel through a wormhole in space...",
            releaseDate: new DateTime(2014, 11, 5),
            tmdbVoteAverage: 8.4,
            status: WatchStatus.PlanToWatch,
            runtimeMinutes: 169,
            genres: new List<string> { "Adventure", "Drama", "Science Fiction" });

        Assert.Equal(userId, item.UserId);
        Assert.Equal(tenantId, item.TenantId);
        Assert.Equal(157336, item.TmdbId);
        Assert.Equal(MediaType.Movie, item.Type);
        Assert.Equal("Interstellar", item.Title);
        Assert.Equal(WatchStatus.PlanToWatch, item.Status);
        Assert.Equal(169, item.RuntimeMinutes);
        Assert.Equal(1, item.CurrentSeason);
        Assert.Equal(0, item.CurrentEpisode);
    }

    [Fact]
    public void WatchItemDocument_TvSeries_ProgressAndNextEpisode()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var item = WatchItemDocument.Create(
            userId: userId,
            tenantId: tenantId,
            tmdbId: 1399, // Game of Thrones
            type: MediaType.TvSeries,
            title: "Game of Thrones",
            status: WatchStatus.Watching,
            currentSeason: 1,
            currentEpisode: 0,
            totalSeasons: 8,
            totalEpisodes: 73,
            runtimeMinutes: 60,
            genres: new List<string> { "Sci-Fi & Fantasy", "Drama" });

        Assert.Equal("Season 1, Ep 1", item.GetNextEpisodeLabel());

        item.UpdateProgress(1, 5, 8, 73);
        Assert.Equal(1, item.CurrentSeason);
        Assert.Equal(5, item.CurrentEpisode);
        Assert.Equal("Season 1, Ep 6", item.GetNextEpisodeLabel());
        Assert.Equal(WatchStatus.Watching, item.Status);
        Assert.Equal(5 * 60, item.GetEstimatedWatchedMinutes());
    }

    [Fact]
    public void WatchItemDocument_RateAndReview_UpdatesFieldsAndCompletesStatus()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var item = WatchItemDocument.Create(
            userId: userId,
            tenantId: tenantId,
            tmdbId: 100,
            type: MediaType.Movie,
            title: "Fight Club",
            status: WatchStatus.PlanToWatch,
            runtimeMinutes: 139,
            genres: new List<string> { "Drama" });

        item.RateAndReview(9, "Masterpiece movie!", new DateTime(2026, 1, 15));
        item.ChangeStatus(WatchStatus.Completed);

        Assert.Equal(9, item.UserRating);
        Assert.Equal("Masterpiece movie!", item.UserReview);
        Assert.Equal(new DateTime(2026, 1, 15), item.WatchedDate);
        Assert.Equal(WatchStatus.Completed, item.Status);
    }

    [Fact]
    public void FollowedPersonDocument_Create_And_UpdateNotes()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var person = FollowedPersonDocument.Create(
            userId: userId,
            tenantId: tenantId,
            tmdbPersonId: 525, // Christopher Nolan
            name: "Christopher Nolan",
            department: "Directing",
            profilePath: "/xuAIuYSmsUzKlUMBFGVZaWsY3Z5.jpg",
            notes: "Favorite director!");

        Assert.Equal(userId, person.UserId);
        Assert.Equal(525, person.TmdbPersonId);
        Assert.Equal("Christopher Nolan", person.Name);
        Assert.Equal("Directing", person.KnownForDepartment);
        Assert.Equal("Favorite director!", person.Notes);

        person.UpdateNotes("Updated notes about his latest movie.");
        Assert.Equal("Updated notes about his latest movie.", person.Notes);
    }

    [Fact]
    public void CustomMediaListDocument_Add_And_Remove_Items()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var list = CustomMediaListDocument.Create(
            userId: userId,
            tenantId: tenantId,
            title: "Friday Night Movies",
            description: "Cozy movie night titles");

        Assert.Equal("Friday Night Movies", list.Title);
        Assert.Empty(list.Items);

        list.AddItem(550, MediaType.Movie, "Fight Club", "/poster.jpg", 1999);
        list.AddItem(157336, MediaType.Movie, "Interstellar", "/poster2.jpg", 2014);

        Assert.Equal(2, list.Items.Count);

        // Duplicate add is ignored
        list.AddItem(550, MediaType.Movie, "Fight Club");
        Assert.Equal(2, list.Items.Count);

        // Remove item
        list.RemoveItem(550, MediaType.Movie);
        Assert.Single(list.Items);
        Assert.Equal(157336, list.Items[0].TmdbId);
    }

    [Fact]
    public void AddWatchItemCommandValidator_Fails_When_Invalid()
    {
        var validator = new AddWatchItemCommandValidator();

        var invalidCommand = new AddWatchItemCommand(
            UserId: Guid.Empty,
            TenantId: Guid.Empty,
            TmdbId: 0, // invalid TMDb ID
            Type: MediaType.Movie,
            Title: "", // empty title
            Status: WatchStatus.PlanToWatch);

        var result = validator.TestValidate(invalidCommand);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.TmdbId);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void UpdateWatchProgressCommandValidator_Fails_When_Negative()
    {
        var validator = new UpdateWatchProgressCommandValidator();

        var invalidCommand = new UpdateWatchProgressCommand(
            WatchItemId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Season: -1, // negative season
            Episode: -1); // negative episode

        var result = validator.TestValidate(invalidCommand);

        result.ShouldHaveValidationErrorFor(x => x.Season);
        result.ShouldHaveValidationErrorFor(x => x.Episode);
    }

    [Fact]
    public void RateWatchItemCommandValidator_Fails_When_RatingOutOfRange()
    {
        var validator = new RateWatchItemCommandValidator();

        var invalidCommand = new RateWatchItemCommand(
            WatchItemId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Rating: 15, // Out of range (> 10)
            Review: null,
            WatchedDate: null);

        var result = validator.TestValidate(invalidCommand);

        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void CreateCustomListCommandValidator_Fails_When_EmptyTitle()
    {
        var validator = new CreateCustomListCommandValidator();

        var invalidCommand = new CreateCustomListCommand(
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Title: ""); // empty title

        var result = validator.TestValidate(invalidCommand);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void WatchItemDocument_Create_WithWatchedDateAndRating()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var watchedDate = DateTime.UtcNow.AddYears(-3);

        var item = WatchItemDocument.Create(
            userId: userId,
            tenantId: tenantId,
            tmdbId: 1429, // Attack on Titan
            type: MediaType.Anime,
            title: "Attack on Titan",
            status: WatchStatus.Completed,
            currentSeason: 4,
            currentEpisode: 87,
            totalSeasons: 4,
            totalEpisodes: 87,
            watchedDate: watchedDate,
            userRating: 10,
            userReview: "Watched 3 years ago, absolute masterpiece!");

        Assert.Equal(WatchStatus.Completed, item.Status);
        Assert.Equal(watchedDate, item.WatchedDate);
        Assert.Equal(10, item.UserRating);
        Assert.Equal("Watched 3 years ago, absolute masterpiece!", item.UserReview);
        Assert.Equal(4, item.CurrentSeason);
        Assert.Equal(87, item.CurrentEpisode);
    }

    [Fact]
    public void GetSeasonEpisodesQueryValidator_Validates_Correctly()
    {
        var validator = new AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQueryValidator();

        var invalidCommand = new AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQuery(0, -1);
        var invalidResult = validator.TestValidate(invalidCommand);
        invalidResult.ShouldHaveValidationErrorFor(x => x.TvShowId);
        invalidResult.ShouldHaveValidationErrorFor(x => x.SeasonNumber);

        var validCommand = new AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQuery(1429, 1);
        var validResult = validator.TestValidate(validCommand);
        validResult.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EstimatedDuration_Calculates_AccurateMinutes_ForAnime_And_Series()
    {
        var anime = WatchItemDocument.Create(
            userId: Guid.NewGuid(),
            tenantId: Guid.NewGuid(),
            tmdbId: 1429,
            type: MediaType.Anime,
            title: "Attack on Titan",
            status: WatchStatus.Completed,
            currentSeason: 4,
            currentEpisode: 87,
            totalSeasons: 4,
            totalEpisodes: 87,
            runtimeMinutes: null // TMDb returns null/0 for Attack on Titan
        );

        // 87 episodes * 24 min (default anime) = 2088 minutes (~34.8 hours)
        Assert.Equal(2088, anime.GetEstimatedWatchedMinutes());

        var series = WatchItemDocument.Create(
            userId: Guid.NewGuid(),
            tenantId: Guid.NewGuid(),
            tmdbId: 1399,
            type: MediaType.TvSeries,
            title: "Game of Thrones",
            status: WatchStatus.Completed,
            currentSeason: 8,
            currentEpisode: 73,
            totalSeasons: 8,
            totalEpisodes: 73,
            runtimeMinutes: null // defaults to 45 min
        );

        // 73 episodes * 45 min = 3285 minutes
        Assert.Equal(3285, series.GetEstimatedWatchedMinutes());
    }

    [Fact]
    public async Task GetMediaStats_Calculates_AccurateHours_ForAnime_WithoutMixingSeries()
    {
        var userId = Guid.NewGuid();
        var anime = WatchItemDocument.Create(
            userId: userId,
            tenantId: Guid.NewGuid(),
            tmdbId: 1429,
            type: MediaType.Anime,
            title: "Attack on Titan",
            status: WatchStatus.Completed,
            currentSeason: 4,
            currentEpisode: 87,
            totalSeasons: 4,
            totalEpisodes: 87,
            runtimeMinutes: null
        );

        var mockUow = new Moq.Mock<AzureTest.Application.Interfaces.Data.NoSql.INoSqlUnitOfWork>();
        var mockWatchItems = new Moq.Mock<AzureTest.Application.Interfaces.Data.NoSql.IWatchItemRepository>();
        var mockFollowed = new Moq.Mock<AzureTest.Application.Interfaces.Data.NoSql.IFollowedPersonRepository>();

        mockWatchItems.Setup(r => r.GetUserWatchListAsync(userId, null, null, Moq.It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WatchItemDocument> { anime });
        mockFollowed.Setup(r => r.GetUserFollowedPeopleAsync(userId, Moq.It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AzureTest.Domain.Documents.Media.FollowedPersonDocument>());

        mockUow.Setup(u => u.WatchItems).Returns(mockWatchItems.Object);
        mockUow.Setup(u => u.FollowedPeople).Returns(mockFollowed.Object);

        var handler = new AzureTest.Application.Media.Queries.GetMediaStats.GetMediaStatsQueryHandler(mockUow.Object);
        var result = await handler.HandleAsync(new AzureTest.Application.Media.Queries.GetMediaStats.GetMediaStatsQuery(userId));

        Assert.True(result.IsSuccess);
        var stats = result.Value;

        Assert.Equal(1, stats.TotalAnimeWatched);
        Assert.Equal(0, stats.TotalSeriesWatched);
        Assert.Equal(0, stats.TotalMoviesWatched);
        Assert.Equal(87, stats.TotalEpisodesWatched);
        Assert.Equal(34, stats.TotalEstimatedHoursWatched); // 2088 / 60 = 34 hours, NOT 65!

        var animeBreakdown = stats.TypeBreakdown.FirstOrDefault(t => t.MediaType == "Anime");
        Assert.NotNull(animeBreakdown);
        Assert.Equal(1, animeBreakdown.Count);
        Assert.Equal(34, animeBreakdown.HoursWatched);

        var seriesBreakdown = stats.TypeBreakdown.FirstOrDefault(t => t.MediaType == "Series");
        Assert.NotNull(seriesBreakdown);
        Assert.Equal(0, seriesBreakdown.Count);
        Assert.Equal(0, seriesBreakdown.HoursWatched);
    }

    [Fact]
    public void WatchItemDocument_EpisodeRewatch_IsIsolated_FromSeasonAndTitle()
    {
        var item = WatchItemDocument.Create(
            userId: Guid.NewGuid(),
            tenantId: Guid.NewGuid(),
            tmdbId: 1668, // Friends
            type: MediaType.TvSeries,
            title: "Friends",
            status: WatchStatus.Completed,
            currentSeason: 10,
            currentEpisode: 17,
            totalSeasons: 10,
            totalEpisodes: 236
        );

        // Entire title and seasons start at 0
        Assert.Equal(0, item.RewatchCount);
        Assert.Equal(0, item.GetSeasonRewatchCount(1));
        Assert.Equal(0, item.GetEpisodeRewatchCount(1, 1));

        // Rewatch only S1E1
        item.RewatchEpisode(1, 1);

        // Only S1E1 count is incremented
        Assert.Equal(1, item.GetEpisodeRewatchCount(1, 1));
        Assert.Equal(0, item.GetEpisodeRewatchCount(1, 2));
        Assert.Equal(0, item.GetSeasonRewatchCount(1));
        Assert.Equal(0, item.RewatchCount);

        // Rewatch S1E1 again
        item.RewatchEpisode(1, 1);
        Assert.Equal(2, item.GetEpisodeRewatchCount(1, 1));
        Assert.Equal(0, item.RewatchCount);

        // Undo episode rewatch
        item.UndoEpisodeRewatch(1, 1);
        Assert.Equal(1, item.GetEpisodeRewatchCount(1, 1));

        item.UndoEpisodeRewatch(1, 1);
        Assert.Equal(0, item.GetEpisodeRewatchCount(1, 1));
        Assert.Equal(0, item.RewatchCount);
    }

    [Fact]
    public void WatchItemDocument_SeasonRewatch_IsIsolated_FromTitleAndEpisodes()
    {
        var item = WatchItemDocument.Create(
            userId: Guid.NewGuid(),
            tenantId: Guid.NewGuid(),
            tmdbId: 1668,
            type: MediaType.TvSeries,
            title: "Friends",
            status: WatchStatus.Completed,
            currentSeason: 10,
            currentEpisode: 17,
            totalSeasons: 10,
            totalEpisodes: 236
        );

        // Rewatch Season 2 only
        item.RewatchSeason(2);

        Assert.Equal(1, item.GetSeasonRewatchCount(2));
        Assert.Equal(0, item.GetSeasonRewatchCount(1));
        Assert.Equal(0, item.RewatchCount);
        Assert.Equal(0, item.GetEpisodeRewatchCount(2, 1));

        // Undo Season 2 rewatch
        item.UndoSeasonRewatch(2);
        Assert.Equal(0, item.GetSeasonRewatchCount(2));
        Assert.Equal(0, item.RewatchCount);
    }

    [Fact]
    public void WatchItemDocument_EntireTitleRewatch_IsIsolated_FromSeasonsAndEpisodes()
    {
        var item = WatchItemDocument.Create(
            userId: Guid.NewGuid(),
            tenantId: Guid.NewGuid(),
            tmdbId: 1668,
            type: MediaType.TvSeries,
            title: "Friends",
            status: WatchStatus.Completed,
            currentSeason: 10,
            currentEpisode: 17,
            totalSeasons: 10,
            totalEpisodes: 236
        );

        // Rewatch entire series
        item.IncrementRewatch();

        Assert.Equal(1, item.RewatchCount);
        Assert.Equal(0, item.GetSeasonRewatchCount(1));
        Assert.Equal(0, item.GetEpisodeRewatchCount(1, 1));

        item.UndoRewatch();
        Assert.Equal(0, item.RewatchCount);
    }

    [Fact]
    public async Task RewatchItemCommandHandler_Handles_ScopedRewatchAndUndo()
    {
        var userId = Guid.NewGuid();
        var item = WatchItemDocument.Create(
            userId: userId,
            tenantId: Guid.NewGuid(),
            tmdbId: 1668,
            type: MediaType.TvSeries,
            title: "Friends",
            status: WatchStatus.Completed,
            currentSeason: 10,
            currentEpisode: 17,
            totalSeasons: 10,
            totalEpisodes: 236
        );

        var mockUow = new Mock<AzureTest.Application.Interfaces.Data.NoSql.INoSqlUnitOfWork>();
        var mockRepo = new Mock<AzureTest.Application.Interfaces.Data.NoSql.IWatchItemRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        mockRepo.Setup(r => r.UpdateAsync(item.Id, item, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockUow.Setup(u => u.WatchItems).Returns(mockRepo.Object);

        var handler = new AzureTest.Application.Media.Commands.RewatchItem.RewatchItemCommandHandler(mockUow.Object);

        // 1. Rewatch Episode 5 of Season 2
        var epRewatchCmd = new AzureTest.Application.Media.Commands.RewatchItem.RewatchItemCommand(
            UserId: userId,
            WatchItemId: item.Id,
            Scope: AzureTest.Application.Media.Commands.RewatchItem.RewatchTargetScope.Episode,
            TargetSeason: 2,
            TargetEpisode: 5);

        var res1 = await handler.HandleAsync(epRewatchCmd);
        Assert.True(res1.IsSuccess);
        Assert.Equal(1, item.GetEpisodeRewatchCount(2, 5));
        Assert.Equal(0, item.GetSeasonRewatchCount(2));
        Assert.Equal(0, item.RewatchCount);

        // 2. Undo Episode 5 rewatch
        var epUndoCmd = epRewatchCmd with { IsUndo = true };
        var res2 = await handler.HandleAsync(epUndoCmd);
        Assert.True(res2.IsSuccess);
        Assert.Equal(0, item.GetEpisodeRewatchCount(2, 5));
        Assert.Equal(0, item.RewatchCount);

        // 3. Rewatch Season 3
        var seasonRewatchCmd = new AzureTest.Application.Media.Commands.RewatchItem.RewatchItemCommand(
            UserId: userId,
            WatchItemId: item.Id,
            Scope: AzureTest.Application.Media.Commands.RewatchItem.RewatchTargetScope.Season,
            TargetSeason: 3);

        var res3 = await handler.HandleAsync(seasonRewatchCmd);
        Assert.True(res3.IsSuccess);
        Assert.Equal(1, item.GetSeasonRewatchCount(3));
        Assert.Equal(0, item.RewatchCount);

        // 4. Undo Season 3 rewatch
        var seasonUndoCmd = seasonRewatchCmd with { IsUndo = true };
        var res4 = await handler.HandleAsync(seasonUndoCmd);
        Assert.True(res4.IsSuccess);
        Assert.Equal(0, item.GetSeasonRewatchCount(3));
    }
}

