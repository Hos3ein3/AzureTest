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
        public const int DefaultAnimeEpisodeDurationMinutes = 24;
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

    private WatchStatus _status = WatchStatus.PlanToWatch;
    public WatchStatus Status 
    { 
        get => _status == 0 ? WatchStatus.PlanToWatch : _status; 
        private set => _status = value; 
    }

    public bool IsMovie => Type == MediaType.Movie || (Type == MediaType.Anime && TotalEpisodes <= 1 && TotalSeasons <= 1);
    public bool IsSeries => !IsMovie;

    public int CurrentSeason { get; private set; }
    public int CurrentEpisode { get; private set; }
    public int TotalSeasons { get; private set; }
    public int TotalEpisodes { get; private set; }
    public int? RuntimeMinutes { get; private set; }
    public int RewatchCount { get; private set; } = 0;
    public Dictionary<string, int> SeasonRewatches { get; private set; } = new();
    public Dictionary<string, int> EpisodeRewatches { get; private set; } = new();

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
        string? userReview = null,
        int rewatchCount = 0)
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
            RewatchCount = Math.Max(0, rewatchCount),
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

        if (totalSeasons.HasValue && totalSeasons.Value > TotalSeasons)
        {
            TotalSeasons = totalSeasons.Value;
        }

        if (totalEpisodes.HasValue && totalEpisodes.Value > TotalEpisodes)
        {
            TotalEpisodes = totalEpisodes.Value;
        }

        LastWatchedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        var isEntireSeriesCompleted = TotalEpisodes > 1
            && CurrentEpisode >= TotalEpisodes
            && (TotalSeasons <= 1 || CurrentSeason >= TotalSeasons);

        if (isEntireSeriesCompleted)
        {
            Status = WatchStatus.Completed;
            WatchedDate ??= DateTime.UtcNow;
        }
        else
        {
            if (CurrentSeason > 1 || CurrentEpisode > 0)
            {
                Status = WatchStatus.Watching;
            }
            else if (CurrentSeason <= 1 && CurrentEpisode == 0 && Status == WatchStatus.Completed)
            {
                Status = WatchStatus.PlanToWatch;
            }
        }
    }

    public void IncrementRewatch(bool restartWatching = false)
    {
        RewatchCount++;
        UpdatedAt = DateTime.UtcNow;
        LastWatchedAt = DateTime.UtcNow;

        if (restartWatching && !IsMovie)
        {
            CurrentSeason = 1;
            CurrentEpisode = 1;
            Status = WatchStatus.Watching;
        }
        else
        {
            Status = WatchStatus.Completed;
            WatchedDate ??= DateTime.UtcNow;
        }
    }

    public void UndoRewatch()
    {
        if (RewatchCount > 0)
        {
            RewatchCount--;
        }

        if (Status == WatchStatus.Watching)
        {
            Status = WatchStatus.Completed;
            CurrentSeason = TotalSeasons > 0 ? TotalSeasons : 1;
            CurrentEpisode = TotalEpisodes > 0 ? TotalEpisodes : 1;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RewatchSeason(int seasonNumber, bool restartFromSeason = false)
    {
        SeasonRewatches ??= new();
        var key = seasonNumber.ToString();
        SeasonRewatches[key] = SeasonRewatches.GetValueOrDefault(key, 0) + 1;

        UpdatedAt = DateTime.UtcNow;
        LastWatchedAt = DateTime.UtcNow;

        if (restartFromSeason && !IsMovie)
        {
            CurrentSeason = seasonNumber;
            CurrentEpisode = 1;
            Status = WatchStatus.Watching;
        }
    }

    public void UndoSeasonRewatch(int seasonNumber)
    {
        SeasonRewatches ??= new();
        var key = seasonNumber.ToString();
        if (SeasonRewatches.TryGetValue(key, out var count) && count > 0)
        {
            if (count <= 1)
            {
                SeasonRewatches.Remove(key);
            }
            else
            {
                SeasonRewatches[key] = count - 1;
            }
        }

        if (Status == WatchStatus.Watching && CurrentSeason == seasonNumber)
        {
            Status = WatchStatus.Completed;
            CurrentSeason = TotalSeasons > 0 ? TotalSeasons : 1;
            CurrentEpisode = TotalEpisodes > 0 ? TotalEpisodes : 1;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RewatchEpisode(int seasonNumber, int episodeNumber, bool restartFromEpisode = false)
    {
        EpisodeRewatches ??= new();
        var key = $"S{seasonNumber}E{episodeNumber}";
        EpisodeRewatches[key] = EpisodeRewatches.GetValueOrDefault(key, 0) + 1;

        UpdatedAt = DateTime.UtcNow;
        LastWatchedAt = DateTime.UtcNow;

        if (restartFromEpisode && !IsMovie)
        {
            CurrentSeason = seasonNumber;
            CurrentEpisode = episodeNumber;
            Status = WatchStatus.Watching;
        }
    }

    public void UndoEpisodeRewatch(int seasonNumber, int episodeNumber)
    {
        EpisodeRewatches ??= new();
        var key = $"S{seasonNumber}E{episodeNumber}";
        if (EpisodeRewatches.TryGetValue(key, out var count) && count > 0)
        {
            if (count <= 1)
            {
                EpisodeRewatches.Remove(key);
            }
            else
            {
                EpisodeRewatches[key] = count - 1;
            }
        }

        if (Status == WatchStatus.Watching && CurrentSeason == seasonNumber && CurrentEpisode == episodeNumber)
        {
            Status = WatchStatus.Completed;
            CurrentSeason = TotalSeasons > 0 ? TotalSeasons : 1;
            CurrentEpisode = TotalEpisodes > 0 ? TotalEpisodes : 1;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public int GetSeasonRewatchCount(int seasonNumber)
    {
        return SeasonRewatches != null && SeasonRewatches.TryGetValue(seasonNumber.ToString(), out var count) ? count : 0;
    }

    public int GetEpisodeRewatchCount(int seasonNumber, int episodeNumber)
    {
        return EpisodeRewatches != null && EpisodeRewatches.TryGetValue($"S{seasonNumber}E{episodeNumber}", out var count) ? count : 0;
    }

    public void ChangeStatus(WatchStatus newStatus)
    {
        Status = newStatus == 0 ? WatchStatus.PlanToWatch : newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (Status == WatchStatus.Completed)
        {
            WatchedDate ??= DateTime.UtcNow;
            LastWatchedAt ??= DateTime.UtcNow;
            if (IsMovie)
            {
                CurrentEpisode = Math.Max(1, TotalEpisodes);
            }
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
        if (IsMovie)
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
        if (IsMovie)
        {
            var baseMinutes = (RuntimeMinutes.HasValue && RuntimeMinutes.Value > 0)
                ? RuntimeMinutes.Value 
                : Rules.DefaultMovieDurationMinutes;

            var timesWatched = Status == WatchStatus.Completed ? (1 + RewatchCount) : RewatchCount;
            return baseMinutes * Math.Max(0, timesWatched);
        }

        int defaultDuration = Type == MediaType.Anime 
            ? Rules.DefaultAnimeEpisodeDurationMinutes 
            : Rules.DefaultEpisodeDurationMinutes;

        var avgDuration = (RuntimeMinutes.HasValue && RuntimeMinutes.Value > 0) 
            ? RuntimeMinutes.Value 
            : defaultDuration;

        var fullRunEpisodes = TotalEpisodes > 0 ? TotalEpisodes : CurrentEpisode;
        var completedEpisodesFromRepeats = fullRunEpisodes * RewatchCount;

        var currentRunEpisodes = (Status == WatchStatus.Completed && TotalEpisodes > 0)
            ? Math.Max(CurrentEpisode, TotalEpisodes)
            : CurrentEpisode;

        return (completedEpisodesFromRepeats + currentRunEpisodes) * avgDuration;
    }
}
