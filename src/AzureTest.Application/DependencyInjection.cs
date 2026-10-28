

using AzureTest.Application.Abstractions;
using AzureTest.Application.Companies;
using AzureTest.Application.Debts;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Expenses.Units;
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
using AzureTest.Application.Media.Commands.RewatchItem;
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
using AzureTest.Application.Resumes;
using AzureTest.Application.SampleNotes;
using AzureTest.Application.Users;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Contracts.Validators.Resumes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScrutorConfig();

        // Register all FluentValidation validators from the Contracts assembly
        services.AddValidatorsFromAssemblyContaining<CreateResumeDtoValidator>();
        // Register FluentValidation validators from the Application assembly (Commands/Queries)
        services.AddValidatorsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());

        // Register Resume unit aggregators
        services.AddScoped<IResumeCommandUnit, ResumeCommandUnit>();
        services.AddScoped<IResumeQueryUnit, ResumeQueryUnit>();
        services.AddScoped<ICompanyQueryUnit, CompanyQueryUnit>();
        services.AddScoped<ICompanyCommandUnit, CompanyCommandUnit>();
        services.AddScoped<IUserCommandUnit, UserCommandUnit>();
        services.AddScoped<IUserQueryUnit, UserQueryUnit>();
        services.AddScoped<IResumeValidationUnit, ResumeValidationUnit>();
        services.AddScoped<AzureTest.Application.Interfaces.Services.IResumeDocumentService, AzureTest.Application.Services.ResumeDocumentService>();
        services.AddScoped<IResumeFileQueryUnit, ResumeFileQueryUnit>();

        // New Feature Domains
        services.AddScoped<AzureTest.Application.Events.IEventCommandUnit, AzureTest.Application.Events.EventCommandUnit>();
        services.AddScoped<AzureTest.Application.Events.IEventQueryUnit, AzureTest.Application.Events.EventQueryUnit>();
        services.AddScoped<AzureTest.Application.Tasks.ITaskCommandUnit, AzureTest.Application.Tasks.TaskCommandUnit>();
        services.AddScoped<AzureTest.Application.Tasks.ITaskQueryUnit, AzureTest.Application.Tasks.TaskQueryUnit>();
        services.AddScoped<AzureTest.Application.Habits.IHabitCommandUnit, AzureTest.Application.Habits.HabitCommandUnit>();
        services.AddScoped<AzureTest.Application.Habits.IHabitQueryUnit, AzureTest.Application.Habits.HabitQueryUnit>();
        services.AddScoped<AzureTest.Application.Tickets.ITicketCommandUnit, AzureTest.Application.Tickets.TicketCommandUnit>();
        services.AddScoped<AzureTest.Application.Tickets.ITicketQueryUnit, AzureTest.Application.Tickets.TicketQueryUnit>();
        
        services.AddScoped<IDebtCommandUnit, DebtCommandUnit>();
        services.AddScoped<IDebtQueryUnit, DebtQueryUnit>();
        services.AddScoped<IExpenseQueryUnit, ExpenseQueryUnit>();
        services.AddScoped<IExpenseCommandUnit, ExpenseCommandUnit>();
        services.AddScoped<AzureTest.Application.Permissions.IPermissionQueryUnit, AzureTest.Application.Permissions.PermissionQueryUnit>();
        services.AddScoped<AzureTest.Application.Permissions.IPermissionCommandUnit, AzureTest.Application.Permissions.PermissionCommandUnit>();

        // SampleNote dev playground
        services.AddScoped<ISampleNoteCommandUnit, SampleNoteCommandUnit>();
        services.AddScoped<ISampleNoteQueryUnit, SampleNoteQueryUnit>();

        // Job Crawler
        services.AddScoped<AzureTest.Application.JobCrawler.IJobCrawlerCommandUnit, AzureTest.Application.JobCrawler.JobCrawlerCommandUnit>();
        services.AddScoped<AzureTest.Application.JobCrawler.IJobCrawlerQueryUnit, AzureTest.Application.JobCrawler.JobCrawlerQueryUnit>();

        // House Crawler
        services.AddScoped<AzureTest.Application.HouseCrawler.Interfaces.IHouseCrawlerCommandUnit, AzureTest.Application.HouseCrawler.HouseCrawlerCommandUnit>();
        services.AddScoped<AzureTest.Application.HouseCrawler.Interfaces.IHouseCrawlerQueryUnit, AzureTest.Application.HouseCrawler.HouseCrawlerQueryUnit>();

        // Idea Tracker
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaCommandUnit, AzureTest.Application.IdeaTracker.IdeaCommandUnit>();
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaQueryUnit, AzureTest.Application.IdeaTracker.IdeaQueryUnit>();
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaPageCommandUnit, AzureTest.Application.IdeaTracker.IdeaPageCommandUnit>();
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaPageQueryUnit, AzureTest.Application.IdeaTracker.IdeaPageQueryUnit>();
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaSectionCommandUnit, AzureTest.Application.IdeaTracker.IdeaSectionCommandUnit>();
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaSectionQueryUnit, AzureTest.Application.IdeaTracker.IdeaSectionQueryUnit>();



        // Pomodoro
        services.AddScoped<AzureTest.Application.Pomodoro.IPomodoroCommandUnit, AzureTest.Application.Pomodoro.PomodoroCommandUnit>();
        services.AddScoped<AzureTest.Application.Pomodoro.IPomodoroQueryUnit, AzureTest.Application.Pomodoro.PomodoroQueryUnit>();

        // Football Fantasy
        services.AddScoped<AzureTest.Application.FootballFantasy.IFootballFantasyQueryUnit, AzureTest.Application.FootballFantasy.FootballFantasyQueryUnit>();
        services.AddScoped<AzureTest.Application.FootballFantasy.IFootballFantasyCommandUnit, AzureTest.Application.FootballFantasy.FootballFantasyCommandUnit>();

        // Food & Cooking
        services.AddScoped<AzureTest.Application.Food.IFoodQueryUnit, AzureTest.Application.Food.FoodQueryUnit>();
        services.AddScoped<AzureTest.Application.Food.IFoodCommandUnit, AzureTest.Application.Food.FoodCommandUnit>();

        // Forex Trading & Market Analysis
        AzureTest.Application.Forex.ForexServiceRegistration.AddForexModule(services);

        // Crypto Markets & Algorithmic Trading
        AzureTest.Application.Crypto.CryptoServiceRegistration.AddCryptoModule(services);

        // Universal Paper Trading
        AzureTest.Application.Trading.TradingServiceRegistration.AddTradingModule(services);

        // Media (TMDb Movies, Series, Anime, Actors)
        
        // ── Command Handlers ──────────────────────────────────────────────────
        services.AddScoped<ICommandHandler<AddWatchItemCommand, Result<Guid>>, AddWatchItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateWatchProgressCommand, Result>, UpdateWatchProgressCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeWatchStatusCommand, Result>, ChangeWatchStatusCommandHandler>();
        services.AddScoped<ICommandHandler<RateWatchItemCommand, Result>, RateWatchItemCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteWatchItemCommand, Result>, DeleteWatchItemCommandHandler>();
        services.AddScoped<ICommandHandler<RewatchItemCommand, Result>, RewatchItemCommandHandler>();
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
        services.AddScoped<IValidator<RewatchItemCommand>, RewatchItemCommandValidator>();
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
        services.AddScoped<IQueryHandler<AzureTest.Application.Media.Queries.GetSmartRecommendations.GetSmartRecommendationsQuery, Result<MediaRecommendationsDto>>, AzureTest.Application.Media.Queries.GetSmartRecommendations.GetSmartRecommendationsQueryHandler>();

        // ── Units ─────────────────────────────────────────────────────────────
        services.AddScoped<IMediaCommandUnit, MediaCommandUnit>();
        services.AddScoped<IMediaQueryUnit, MediaQueryUnit>();


        return services;
    }
}
