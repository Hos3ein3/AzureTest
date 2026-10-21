using System;
using System.Collections.Generic;
using AzureTest.Application.Abstractions;
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
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Media;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Application.Media;

public static class DependencyInjection
{
    public static IServiceCollection AddMediaModule(this IServiceCollection services)
    {
        // ── Command Handlers ──────────────────────────────────────────────────
        services.AddScoped<ICommandHandler<AddWatchItemCommand, Result<Guid>>, AddWatchItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateWatchProgressCommand, Result>, UpdateWatchProgressCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeWatchStatusCommand, Result>, ChangeWatchStatusCommandHandler>();
        services.AddScoped<ICommandHandler<RateWatchItemCommand, Result>, RateWatchItemCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteWatchItemCommand, Result>, DeleteWatchItemCommandHandler>();
        services.AddScoped<ICommandHandler<FollowPersonCommand, Result<Guid>>, FollowPersonCommandHandler>();
        services.AddScoped<ICommandHandler<UnfollowPersonCommand, Result>, UnfollowPersonCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateFollowedPersonNotesCommand, Result>, UpdateFollowedPersonNotesCommandHandler>();
        services.AddScoped<ICommandHandler<CreateCustomListCommand, Result<Guid>>, CreateCustomListCommandHandler>();
        services.AddScoped<ICommandHandler<AddItemToCustomListCommand, Result>, AddItemToCustomListCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveItemFromCustomListCommand, Result>, RemoveItemFromCustomListCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteCustomListCommand, Result>, DeleteCustomListCommandHandler>();
        services.AddScoped<ICommandHandler<ImportMediaDataCommand, Result<MediaImportResultDto>>, ImportMediaDataCommandHandler>();

        // ── Validators ────────────────────────────────────────────────────────
        services.AddScoped<IValidator<AddWatchItemCommand>, AddWatchItemCommandValidator>();
        services.AddScoped<IValidator<UpdateWatchProgressCommand>, UpdateWatchProgressCommandValidator>();
        services.AddScoped<IValidator<ChangeWatchStatusCommand>, ChangeWatchStatusCommandValidator>();
        services.AddScoped<IValidator<RateWatchItemCommand>, RateWatchItemCommandValidator>();
        services.AddScoped<IValidator<DeleteWatchItemCommand>, DeleteWatchItemCommandValidator>();
        services.AddScoped<IValidator<FollowPersonCommand>, FollowPersonCommandValidator>();
        services.AddScoped<IValidator<UnfollowPersonCommand>, UnfollowPersonCommandValidator>();
        services.AddScoped<IValidator<UpdateFollowedPersonNotesCommand>, UpdateFollowedPersonNotesCommandValidator>();
        services.AddScoped<IValidator<CreateCustomListCommand>, CreateCustomListCommandValidator>();
        services.AddScoped<IValidator<AddItemToCustomListCommand>, AddItemToCustomListCommandValidator>();
        services.AddScoped<IValidator<RemoveItemFromCustomListCommand>, RemoveItemFromCustomListCommandValidator>();
        services.AddScoped<IValidator<DeleteCustomListCommand>, DeleteCustomListCommandValidator>();
        services.AddScoped<IValidator<ImportMediaDataCommand>, ImportMediaDataCommandValidator>();

        // ── Query Handlers ────────────────────────────────────────────────────
        services.AddScoped<IQueryHandler<GetWatchListQuery, Result<PagedResult<WatchItemDto, WatchListFilter>>>, GetWatchListQueryHandler>();
        services.AddScoped<IQueryHandler<GetWatchItemByIdQuery, Result<WatchItemDto>>, GetWatchItemByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetDiscoverMediaQuery, Result<TmdbDiscoverDto>>, GetDiscoverMediaQueryHandler>();
        services.AddScoped<IQueryHandler<SearchMediaQuery, Result<IReadOnlyList<TmdbSearchResultDto>>>, SearchMediaQueryHandler>();
        services.AddScoped<IQueryHandler<GetMediaDetailsQuery, Result<TmdbTitleDetailsDto>>, GetMediaDetailsQueryHandler>();
        services.AddScoped<IQueryHandler<GetFollowedPeopleQuery, Result<IReadOnlyList<FollowedPersonDto>>>, GetFollowedPeopleQueryHandler>();
        services.AddScoped<IQueryHandler<GetPersonDetailsQuery, Result<FollowedPersonDto>>, GetPersonDetailsQueryHandler>();
        services.AddScoped<IQueryHandler<GetMediaCalendarQuery, Result<MediaCalendarDto>>, GetMediaCalendarQueryHandler>();
        services.AddScoped<IQueryHandler<GetMovieNewsQuery, Result<IReadOnlyList<MovieNewsItemDto>>>, GetMovieNewsQueryHandler>();
        services.AddScoped<IQueryHandler<GetCustomListsQuery, Result<IReadOnlyList<CustomMediaListDto>>>, GetCustomListsQueryHandler>();
        services.AddScoped<IQueryHandler<GetCustomListByIdQuery, Result<CustomMediaListDto>>, GetCustomListByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetMediaStatsQuery, Result<MediaStatsDto>>, GetMediaStatsQueryHandler>();
        services.AddScoped<IQueryHandler<ExportMediaDataQuery, Result<MediaExportDto>>, ExportMediaDataQueryHandler>();
        services.AddScoped<IQueryHandler<AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQuery, Result<IReadOnlyList<TvEpisodeSummaryDto>>>, AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQueryHandler>();
        services.AddScoped<IValidator<AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQuery>, AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQueryValidator>();

        // ── Units ─────────────────────────────────────────────────────────────
        services.AddScoped<IMediaCommandUnit, MediaCommandUnit>();
        services.AddScoped<IMediaQueryUnit, MediaQueryUnit>();

        return services;
    }
}
