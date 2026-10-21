using System;
using System.Collections.Generic;
using AzureTest.Domain.Media.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents.Media;

public class WatchItemDocument : ITenantEntity
{
    public static class Rules
    {
        public const int TitleMaxLength = 300;
        public const int ReviewMaxLength = 2000;
        public const int MinRating = 1;
        public const int MaxRating = 10;
        public const int MinSeason = 1;
        public const int MaxSeason = 200;
        public const int MinEpisode = 0;
        public const int MaxEpisode = 5000;
        public const int DefaultEpisodeDurationMinutes = 45;
        public const int DefaultMovieDurationMinutes = 110;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid UserId { get; private set; }
    public int TmdbId { get; private set; }
    public MediaType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? OriginalTitle { get; private set; }
    public string? PosterPath { get; private set; }
    public string? BackdropPath { get; private set; }
    public string? Overview { get; private set; }
    public DateTime? ReleaseDate { get; private set; }
    public double? TmdbVoteAverage { get; private set; }
    public WatchStatus Status { get; private set; }

    public int CurrentSeason { get; private set; }
    public int CurrentEpisode { get; private set; }
    public int TotalSeasons { get; private set; }
    public int TotalEpisodes { get; private set; }
    public int? RuntimeMinutes { get; private set; }

    public int? UserRating { get; private set; }
    public string? UserReview { get; private set; }
    public DateTime? WatchedDate { get; private set; }
    public DateTime? LastWatchedAt { get; private set; }

    public List<string> Genres { get; private set; } = new();

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private WatchItemDocument() { }

    public static WatchItemDocument Create(
        Guid userId,
        Guid tenantId,
        int tmdbId,
        MediaType type,
        string title,
        string? originalTitle = null,
        string? posterPath = null,
        string? backdropPath = null,
        string? overview = null,
        DateTime? releaseDate = null,
        double? tmdbVoteAverage = null,
        WatchStatus status = WatchStatus.PlanToWatch,
        int currentSeason = 1,
        int currentEpisode = 0,
        int totalSeasons = 1,
        int totalEpisodes = 1,
        int? runtimeMinutes = null,
        List<string>? genres = null,
        DateTime? watchedDate = null,
        int? userRating = null,
        string? userReview = null)
    {
        var item = new WatchItemDocument
        {
            UserId = userId,
            TenantId = tenantId,
            TmdbId = tmdbId,
            Type = type,
            Title = string.IsNullOrWhiteSpace(title) ? "Untitled" : (title.Length > Rules.TitleMaxLength ? title[..Rules.TitleMaxLength] : title),
            OriginalTitle = originalTitle,
            PosterPath = posterPath,
            BackdropPath = backdropPath,
            Overview = overview,
            ReleaseDate = releaseDate,
            TmdbVoteAverage = tmdbVoteAverage,
            Status = status,
            CurrentSeason = Math.Clamp(currentSeason, Rules.MinSeason, Rules.MaxSeason),
            CurrentEpisode = Math.Clamp(currentEpisode, Rules.MinEpisode, Rules.MaxEpisode),
            TotalSeasons = Math.Max(1, totalSeasons),
            TotalEpisodes = Math.Max(0, totalEpisodes),
            RuntimeMinutes = runtimeMinutes,
            Genres = genres ?? new List<string>(),
            WatchedDate = watchedDate ?? (status == WatchStatus.Completed ? DateTime.UtcNow : null),
            LastWatchedAt = watchedDate ?? (status == WatchStatus.Completed ? DateTime.UtcNow : null),
            UserRating = userRating.HasValue ? Math.Clamp(userRating.Value, Rules.MinRating, Rules.MaxRating) : null,
            UserReview = !string.IsNullOrWhiteSpace(userReview) ? (userReview.Length > Rules.ReviewMaxLength ? userReview[..Rules.ReviewMaxLength] : userReview.Trim()) : null,
            CreatedAt = DateTime.UtcNow
        };

        return item;
    }

    public void UpdateProgress(int season, int episode, int? totalSeasons = null, int? totalEpisodes = null)
    {
        CurrentSeason = Math.Clamp(season, Rules.MinSeason, Rules.MaxSeason);
        CurrentEpisode = Math.Clamp(episode, Rules.MinEpisode, Rules.MaxEpisode);

        if (totalSeasons.HasValue && totalSeasons.Value > 0)
        {
            TotalSeasons = totalSeasons.Value;
        }

        if (totalEpisodes.HasValue && totalEpisodes.Value >= 0)
        {
            TotalEpisodes = totalEpisodes.Value;
        }

        LastWatchedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        if (Status == WatchStatus.PlanToWatch && (CurrentSeason > 1 || CurrentEpisode > 0))
        {
            Status = WatchStatus.Watching;
        }
        else if (TotalEpisodes > 0 && CurrentEpisode >= TotalEpisodes && (TotalSeasons <= 1 || CurrentSeason >= TotalSeasons))
        {
            Status = WatchStatus.Completed;
            WatchedDate ??= DateTime.UtcNow;
        }
    }

    public void ChangeStatus(WatchStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == WatchStatus.Completed)
        {
            WatchedDate ??= DateTime.UtcNow;
            LastWatchedAt ??= DateTime.UtcNow;
        }
    }

    public void RateAndReview(int? rating, string? review, DateTime? watchedDate = null)
    {
        if (rating.HasValue)
        {
            UserRating = Math.Clamp(rating.Value, Rules.MinRating, Rules.MaxRating);
        }
        else
        {
            UserRating = null;
        }

        if (!string.IsNullOrWhiteSpace(review))
        {
            UserReview = review.Length > Rules.ReviewMaxLength ? review[..Rules.ReviewMaxLength] : review.Trim();
        }
        else
        {
            UserReview = null;
        }

        if (watchedDate.HasValue)
        {
            WatchedDate = watchedDate.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public string GetNextEpisodeLabel()
    {
        if (Type == MediaType.Movie)
        {
            return Status == WatchStatus.Completed ? "Watched" : "Not watched";
        }

        if (Status == WatchStatus.Completed)
        {
            return "Completed";
        }

        var nextEp = CurrentEpisode + 1;
        return $"Season {CurrentSeason}, Ep {nextEp}";
    }

    public int GetEstimatedWatchedMinutes()
    {
        if (Type == MediaType.Movie)
        {
            return Status == WatchStatus.Completed 
                ? (RuntimeMinutes ?? Rules.DefaultMovieDurationMinutes) 
                : 0;
        }

        var episodesWatched = CurrentEpisode;
        var avgDuration = RuntimeMinutes ?? Rules.DefaultEpisodeDurationMinutes;
        return episodesWatched * avgDuration;
    }
}
