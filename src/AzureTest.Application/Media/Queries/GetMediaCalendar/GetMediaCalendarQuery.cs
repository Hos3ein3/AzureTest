using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Application.Media.Queries.GetMediaCalendar;

public sealed record GetMediaCalendarQuery(Guid UserId, int? Year = null, int? Month = null) : IQuery<Result<MediaCalendarDto>>;

public sealed class GetMediaCalendarQueryHandler : IQueryHandler<GetMediaCalendarQuery, Result<MediaCalendarDto>>
{
    private readonly INoSqlUnitOfWork _uow;
    private readonly ITmdbService _tmdbService;

    public GetMediaCalendarQueryHandler(INoSqlUnitOfWork uow, ITmdbService tmdbService)
    {
        _uow = uow;
        _tmdbService = tmdbService;
    }

    public async Task<Result<MediaCalendarDto>> HandleAsync(GetMediaCalendarQuery request, CancellationToken ct = default)
    {
        var targetYear = request.Year ?? DateTime.Today.Year;
        var targetMonth = request.Month ?? DateTime.Today.Month;

        var allUserItems = await _uow.WatchItems.GetUserWatchListAsync(request.UserId, ct: ct);

        var events = new List<CalendarEventDto>();
        var upcomingWishlist = new List<CalendarEventDto>();
        var statusAlerts = new List<CalendarEventDto>();

        // Process wishlist items
        var wishlist = allUserItems.Where(x => x.Status == WatchStatus.PlanToWatch).ToList();
        foreach (var item in wishlist)
        {
            if (item.ReleaseDate.HasValue)
            {
                var evt = new CalendarEventDto(
                    item.TmdbId,
                    item.Title,
                    item.Type,
                    item.ReleaseDate.Value,
                    item.PosterPath,
                    "Wishlist Release",
                    $"{item.Type} premiere",
                    null
                );

                if (item.ReleaseDate.Value.Year == targetYear && item.ReleaseDate.Value.Month == targetMonth)
                {
                    events.Add(evt);
                }

                if (item.ReleaseDate.Value >= DateTime.Today)
                {
                    upcomingWishlist.Add(evt);
                }
            }
        }

        // For series being watched, check for upcoming seasons or status updates
        var watchingSeries = allUserItems
            .Where(x => (x.Type == MediaType.TvSeries || x.Type == MediaType.Anime) &&
                        (x.Status == WatchStatus.Watching || x.Status == WatchStatus.Completed))
            .Take(8) // Limit calls to avoid slow loading
            .ToList();

        foreach (var series in watchingSeries)
        {
            try
            {
                var tvDetails = await _tmdbService.GetTvShowDetailsAsync(series.TmdbId, ct);
                if (tvDetails != null)
                {
                    // Check renewal/cancellation
                    if (!string.IsNullOrWhiteSpace(tvDetails.Status))
                    {
                        var statusDesc = tvDetails.Status switch
                        {
                            "Returning Series" => "Renewed / Returning for next season",
                            "Ended" => "Series has concluded (Ended)",
                            "Canceled" => "Series was Canceled",
                            _ => tvDetails.Status
                        };

                        statusAlerts.Add(new CalendarEventDto(
                            series.TmdbId,
                            series.Title,
                            series.Type,
                            DateTime.Today,
                            series.PosterPath,
                            "Status Update",
                            statusDesc,
                            tvDetails.Status
                        ));
                    }

                    // Check for future seasons air dates
                    if (tvDetails.Seasons != null)
                    {
                        foreach (var s in tvDetails.Seasons.Where(s => s.AirDate.HasValue && s.AirDate.Value >= DateTime.Today))
                        {
                            var evt = new CalendarEventDto(
                                series.TmdbId,
                                $"{series.Title} - {s.Name}",
                                series.Type,
                                s.AirDate!.Value,
                                s.PosterPath ?? series.PosterPath,
                                "Season Premiere",
                                $"{s.EpisodeCount} episodes",
                                tvDetails.Status
                            );

                            if (s.AirDate.Value.Year == targetYear && s.AirDate.Value.Month == targetMonth)
                            {
                                events.Add(evt);
                            }

                            upcomingWishlist.Add(evt);
                        }
                    }
                }
            }
            catch
            {
                // Continue if TV details call fails for one item
            }
        }

        return Result.Success(new MediaCalendarDto(
            targetYear,
            targetMonth,
            events.OrderBy(e => e.ReleaseDate).ToList(),
            upcomingWishlist.OrderBy(e => e.ReleaseDate).Take(15).ToList(),
            statusAlerts
        ));
    }
}
