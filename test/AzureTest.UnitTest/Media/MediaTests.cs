using System;
using System.Collections.Generic;
using AzureTest.Application.Media.Commands.AddWatchItem;
using AzureTest.Application.Media.Commands.CreateCustomList;
using AzureTest.Application.Media.Commands.RateWatchItem;
using AzureTest.Application.Media.Commands.UpdateWatchProgress;
using AzureTest.Domain.Documents.Media;
using AzureTest.Domain.Media.Enums;
using FluentValidation.TestHelper;
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
}
