using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.FootballFantasy.Commands;
using AzureTest.Application.FootballFantasy.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.FootballFantasy;

namespace AzureTest.Application.FootballFantasy;

public interface IFootballFantasyQueryUnit
{
    // Fantasy Leagues & Teams
    IQueryHandler<GetFantasyLeaguesQuery, PagedResult<FantasyLeagueDto, FantasyLeagueFilter>> GetFantasyLeagues { get; }
    IQueryHandler<GetFantasyLeagueByIdQuery, Result<FantasyLeagueDto>> GetFantasyLeagueById { get; }
    IQueryHandler<GetPlayersQuery, PagedResult<PlayerDto, PlayerFilter>> GetPlayers { get; }
    IQueryHandler<GetFantasyTeamByLeagueQuery, Result<FantasyTeamDto>> GetFantasyTeamByLeague { get; }
    IQueryHandler<GetFantasyTeamByIdQuery, Result<FantasyTeamDto>> GetFantasyTeamById { get; }
    IQueryHandler<GetFantasyAdminDashboardQuery, Result<FantasyAdminDashboardDto>> GetAdminDashboard { get; }
    IQueryHandler<GetAdminFantasyTeamsQuery, Result<List<AdminFantasyTeamOverviewDto>>> GetAdminFantasyTeams { get; }

    // Real-world Entities Queries
    IQueryHandler<GetCountriesQuery, PagedResult<CountryDto, CountryFilter>> GetCountries { get; }
    IQueryHandler<GetCountryByIdQuery, Result<CountryDto>> GetCountryById { get; }
    IQueryHandler<GetVenuesQuery, PagedResult<VenueDto, VenueFilter>> GetVenues { get; }
    IQueryHandler<GetVenueByIdQuery, Result<VenueDto>> GetVenueById { get; }
    IQueryHandler<GetLeaguesQuery, PagedResult<LeagueDto, LeagueFilter>> GetLeagues { get; }
    IQueryHandler<GetLeagueByIdQuery, Result<LeagueDto>> GetLeagueById { get; }
    IQueryHandler<GetTeamsQuery, PagedResult<TeamDto, TeamFilter>> GetTeams { get; }
    IQueryHandler<GetTeamByIdQuery, Result<TeamDto>> GetTeamById { get; }
    IQueryHandler<GetPlayerByIdQuery, Result<PlayerDto>> GetPlayerById { get; }
    IQueryHandler<GetFixturesQuery, PagedResult<FixtureDto, FixtureFilter>> GetFixtures { get; }
    IQueryHandler<GetFixtureByIdQuery, Result<FixtureDto>> GetFixtureById { get; }
    IQueryHandler<GetCoachesQuery, PagedResult<CoachDto, CoachFilter>> GetCoaches { get; }
    IQueryHandler<GetCoachByIdQuery, Result<CoachDto>> GetCoachById { get; }
    IQueryHandler<GetPlayerMatchStatsQuery, PagedResult<PlayerMatchStatDto, PlayerMatchStatFilter>> GetPlayerMatchStats { get; }
    IQueryHandler<GetPlayerMatchStatByIdQuery, Result<PlayerMatchStatDto>> GetPlayerMatchStatById { get; }

    // Achievements & Manager Profile & Tournament
    IQueryHandler<GetAchievementsQuery, PagedResult<AchievementDto, AchievementFilter>> GetAchievements { get; }
    IQueryHandler<GetAchievementByIdQuery, Result<AchievementDto>> GetAchievementById { get; }
    IQueryHandler<GetManagerProfileQuery, Result<ManagerProfileDto>> GetManagerProfile { get; }
    IQueryHandler<GetLeagueTournamentDetailsQuery, Result<List<FantasyLeagueGroupDto>>> GetLeagueTournamentDetails { get; }
    IQueryHandler<GetPendingJoinRequestsQuery, Result<List<PendingJoinRequestDto>>> GetPendingJoinRequests { get; }

    // Predict Club
    IQueryHandler<GetPredictClubsQuery, PagedResult<PredictClubDto, PredictClubFilter>> GetPredictClubs { get; }
    IQueryHandler<GetPredictClubDetailsQuery, Result<PredictClubDetailsDto>> GetPredictClubDetails { get; }

    // Extended Football Domains
    IQueryHandler<GetLeagueStandingsQuery, PagedResult<LeagueStandingDto, LeagueStandingFilter>> GetLeagueStandings { get; }
    IQueryHandler<GetBookmakersQuery, PagedResult<BookmakerDto, BookmakerFilter>> GetBookmakers { get; }
    IQueryHandler<GetFixtureOddsQuery, PagedResult<FixtureOddDto, FixtureOddFilter>> GetFixtureOdds { get; }
    IQueryHandler<GetPlayerTransfersQuery, PagedResult<PlayerTransferDto, PlayerTransferFilter>> GetPlayerTransfers { get; }
    IQueryHandler<GetPlayerInjuriesQuery, PagedResult<PlayerInjuryDto, PlayerInjuryFilter>> GetPlayerInjuries { get; }
    IQueryHandler<GetTeamTrophiesQuery, PagedResult<TeamTrophyDto, TeamTrophyFilter>> GetTeamTrophies { get; }
    IQueryHandler<GetPlayerTopScorersQuery, PagedResult<PlayerTopScorerDto, PlayerTopScorerFilter>> GetPlayerTopScorers { get; }
}

public sealed class FootballFantasyQueryUnit : IFootballFantasyQueryUnit
{
    public IQueryHandler<GetFantasyLeaguesQuery, PagedResult<FantasyLeagueDto, FantasyLeagueFilter>> GetFantasyLeagues { get; }
    public IQueryHandler<GetFantasyLeagueByIdQuery, Result<FantasyLeagueDto>> GetFantasyLeagueById { get; }
    public IQueryHandler<GetPlayersQuery, PagedResult<PlayerDto, PlayerFilter>> GetPlayers { get; }
    public IQueryHandler<GetFantasyTeamByLeagueQuery, Result<FantasyTeamDto>> GetFantasyTeamByLeague { get; }
    public IQueryHandler<GetFantasyTeamByIdQuery, Result<FantasyTeamDto>> GetFantasyTeamById { get; }
    public IQueryHandler<GetFantasyAdminDashboardQuery, Result<FantasyAdminDashboardDto>> GetAdminDashboard { get; }
    public IQueryHandler<GetAdminFantasyTeamsQuery, Result<List<AdminFantasyTeamOverviewDto>>> GetAdminFantasyTeams { get; }

    public IQueryHandler<GetCountriesQuery, PagedResult<CountryDto, CountryFilter>> GetCountries { get; }
    public IQueryHandler<GetCountryByIdQuery, Result<CountryDto>> GetCountryById { get; }
    public IQueryHandler<GetVenuesQuery, PagedResult<VenueDto, VenueFilter>> GetVenues { get; }
    public IQueryHandler<GetVenueByIdQuery, Result<VenueDto>> GetVenueById { get; }
    public IQueryHandler<GetLeaguesQuery, PagedResult<LeagueDto, LeagueFilter>> GetLeagues { get; }
    public IQueryHandler<GetLeagueByIdQuery, Result<LeagueDto>> GetLeagueById { get; }
    public IQueryHandler<GetTeamsQuery, PagedResult<TeamDto, TeamFilter>> GetTeams { get; }
    public IQueryHandler<GetTeamByIdQuery, Result<TeamDto>> GetTeamById { get; }
    public IQueryHandler<GetPlayerByIdQuery, Result<PlayerDto>> GetPlayerById { get; }
    public IQueryHandler<GetFixturesQuery, PagedResult<FixtureDto, FixtureFilter>> GetFixtures { get; }
    public IQueryHandler<GetFixtureByIdQuery, Result<FixtureDto>> GetFixtureById { get; }
    public IQueryHandler<GetCoachesQuery, PagedResult<CoachDto, CoachFilter>> GetCoaches { get; }
    public IQueryHandler<GetCoachByIdQuery, Result<CoachDto>> GetCoachById { get; }
    public IQueryHandler<GetPlayerMatchStatsQuery, PagedResult<PlayerMatchStatDto, PlayerMatchStatFilter>> GetPlayerMatchStats { get; }
    public IQueryHandler<GetPlayerMatchStatByIdQuery, Result<PlayerMatchStatDto>> GetPlayerMatchStatById { get; }

    public IQueryHandler<GetAchievementsQuery, PagedResult<AchievementDto, AchievementFilter>> GetAchievements { get; }
    public IQueryHandler<GetAchievementByIdQuery, Result<AchievementDto>> GetAchievementById { get; }
    public IQueryHandler<GetManagerProfileQuery, Result<ManagerProfileDto>> GetManagerProfile { get; }
    public IQueryHandler<GetLeagueTournamentDetailsQuery, Result<List<FantasyLeagueGroupDto>>> GetLeagueTournamentDetails { get; }
    public IQueryHandler<GetPendingJoinRequestsQuery, Result<List<PendingJoinRequestDto>>> GetPendingJoinRequests { get; }

    public IQueryHandler<GetPredictClubsQuery, PagedResult<PredictClubDto, PredictClubFilter>> GetPredictClubs { get; }
    public IQueryHandler<GetPredictClubDetailsQuery, Result<PredictClubDetailsDto>> GetPredictClubDetails { get; }

    public IQueryHandler<GetLeagueStandingsQuery, PagedResult<LeagueStandingDto, LeagueStandingFilter>> GetLeagueStandings { get; }
    public IQueryHandler<GetBookmakersQuery, PagedResult<BookmakerDto, BookmakerFilter>> GetBookmakers { get; }
    public IQueryHandler<GetFixtureOddsQuery, PagedResult<FixtureOddDto, FixtureOddFilter>> GetFixtureOdds { get; }
    public IQueryHandler<GetPlayerTransfersQuery, PagedResult<PlayerTransferDto, PlayerTransferFilter>> GetPlayerTransfers { get; }
    public IQueryHandler<GetPlayerInjuriesQuery, PagedResult<PlayerInjuryDto, PlayerInjuryFilter>> GetPlayerInjuries { get; }
    public IQueryHandler<GetTeamTrophiesQuery, PagedResult<TeamTrophyDto, TeamTrophyFilter>> GetTeamTrophies { get; }
    public IQueryHandler<GetPlayerTopScorersQuery, PagedResult<PlayerTopScorerDto, PlayerTopScorerFilter>> GetPlayerTopScorers { get; }

    public FootballFantasyQueryUnit(
        IQueryHandler<GetFantasyLeaguesQuery, PagedResult<FantasyLeagueDto, FantasyLeagueFilter>> getFantasyLeagues,
        IQueryHandler<GetFantasyLeagueByIdQuery, Result<FantasyLeagueDto>> getFantasyLeagueById,
        IQueryHandler<GetPlayersQuery, PagedResult<PlayerDto, PlayerFilter>> getPlayers,
        IQueryHandler<GetFantasyTeamByLeagueQuery, Result<FantasyTeamDto>> getFantasyTeamByLeague,
        IQueryHandler<GetFantasyTeamByIdQuery, Result<FantasyTeamDto>> getFantasyTeamById,
        IQueryHandler<GetFantasyAdminDashboardQuery, Result<FantasyAdminDashboardDto>> getAdminDashboard,
        IQueryHandler<GetAdminFantasyTeamsQuery, Result<List<AdminFantasyTeamOverviewDto>>> getAdminFantasyTeams,
        IQueryHandler<GetCountriesQuery, PagedResult<CountryDto, CountryFilter>> getCountries,
        IQueryHandler<GetCountryByIdQuery, Result<CountryDto>> getCountryById,
        IQueryHandler<GetVenuesQuery, PagedResult<VenueDto, VenueFilter>> getVenues,
        IQueryHandler<GetVenueByIdQuery, Result<VenueDto>> getVenueById,
        IQueryHandler<GetLeaguesQuery, PagedResult<LeagueDto, LeagueFilter>> getLeagues,
        IQueryHandler<GetLeagueByIdQuery, Result<LeagueDto>> getLeagueById,
        IQueryHandler<GetTeamsQuery, PagedResult<TeamDto, TeamFilter>> getTeams,
        IQueryHandler<GetTeamByIdQuery, Result<TeamDto>> getTeamById,
        IQueryHandler<GetPlayerByIdQuery, Result<PlayerDto>> getPlayerById,
        IQueryHandler<GetFixturesQuery, PagedResult<FixtureDto, FixtureFilter>> getFixtures,
        IQueryHandler<GetFixtureByIdQuery, Result<FixtureDto>> getFixtureById,
        IQueryHandler<GetCoachesQuery, PagedResult<CoachDto, CoachFilter>> getCoaches,
        IQueryHandler<GetCoachByIdQuery, Result<CoachDto>> getCoachById,
        IQueryHandler<GetPlayerMatchStatsQuery, PagedResult<PlayerMatchStatDto, PlayerMatchStatFilter>> getPlayerMatchStats,
        IQueryHandler<GetPlayerMatchStatByIdQuery, Result<PlayerMatchStatDto>> getPlayerMatchStatById,
        IQueryHandler<GetAchievementsQuery, PagedResult<AchievementDto, AchievementFilter>> getAchievements,
        IQueryHandler<GetAchievementByIdQuery, Result<AchievementDto>> getAchievementById,
        IQueryHandler<GetManagerProfileQuery, Result<ManagerProfileDto>> getManagerProfile,
        IQueryHandler<GetLeagueTournamentDetailsQuery, Result<List<FantasyLeagueGroupDto>>> getLeagueTournamentDetails,
        IQueryHandler<GetPendingJoinRequestsQuery, Result<List<PendingJoinRequestDto>>> getPendingJoinRequests,
        IQueryHandler<GetPredictClubsQuery, PagedResult<PredictClubDto, PredictClubFilter>> getPredictClubs,
        IQueryHandler<GetPredictClubDetailsQuery, Result<PredictClubDetailsDto>> getPredictClubDetails,
        IQueryHandler<GetLeagueStandingsQuery, PagedResult<LeagueStandingDto, LeagueStandingFilter>> getLeagueStandings,
        IQueryHandler<GetBookmakersQuery, PagedResult<BookmakerDto, BookmakerFilter>> getBookmakers,
        IQueryHandler<GetFixtureOddsQuery, PagedResult<FixtureOddDto, FixtureOddFilter>> getFixtureOdds,
        IQueryHandler<GetPlayerTransfersQuery, PagedResult<PlayerTransferDto, PlayerTransferFilter>> getPlayerTransfers,
        IQueryHandler<GetPlayerInjuriesQuery, PagedResult<PlayerInjuryDto, PlayerInjuryFilter>> getPlayerInjuries,
        IQueryHandler<GetTeamTrophiesQuery, PagedResult<TeamTrophyDto, TeamTrophyFilter>> getTeamTrophies,
        IQueryHandler<GetPlayerTopScorersQuery, PagedResult<PlayerTopScorerDto, PlayerTopScorerFilter>> getPlayerTopScorers)
    {
        GetFantasyLeagues = getFantasyLeagues;
        GetFantasyLeagueById = getFantasyLeagueById;
        GetPlayers = getPlayers;
        GetFantasyTeamByLeague = getFantasyTeamByLeague;
        GetFantasyTeamById = getFantasyTeamById;
        GetAdminDashboard = getAdminDashboard;
        GetAdminFantasyTeams = getAdminFantasyTeams;
        GetCountries = getCountries;
        GetCountryById = getCountryById;
        GetVenues = getVenues;
        GetVenueById = getVenueById;
        GetLeagues = getLeagues;
        GetLeagueById = getLeagueById;
        GetTeams = getTeams;
        GetTeamById = getTeamById;
        GetPlayerById = getPlayerById;
        GetFixtures = getFixtures;
        GetFixtureById = getFixtureById;
        GetCoaches = getCoaches;
        GetCoachById = getCoachById;
        GetPlayerMatchStats = getPlayerMatchStats;
        GetPlayerMatchStatById = getPlayerMatchStatById;
        GetAchievements = getAchievements;
        GetAchievementById = getAchievementById;
        GetManagerProfile = getManagerProfile;
        GetLeagueTournamentDetails = getLeagueTournamentDetails;
        GetPendingJoinRequests = getPendingJoinRequests;
        GetPredictClubs = getPredictClubs;
        GetPredictClubDetails = getPredictClubDetails;
        GetLeagueStandings = getLeagueStandings;
        GetBookmakers = getBookmakers;
        GetFixtureOdds = getFixtureOdds;
        GetPlayerTransfers = getPlayerTransfers;
        GetPlayerInjuries = getPlayerInjuries;
        GetTeamTrophies = getTeamTrophies;
        GetPlayerTopScorers = getPlayerTopScorers;
    }
}

public interface IFootballFantasyCommandUnit
{
    // Fantasy commands
    ICommandHandler<CreateFantasyLeagueCommand, Result<Guid>> CreateFantasyLeague { get; }
    ICommandHandler<CreateFantasyTeamCommand, Result<Guid>> CreateFantasyTeam { get; }
    ICommandHandler<SaveLineupCommand, Result> SaveLineup { get; }
    ICommandHandler<ReviewJoinRequestCommand, Result> ReviewJoinRequest { get; }
    ICommandHandler<WithdrawJoinRequestCommand, Result> WithdrawJoinRequest { get; }
    ICommandHandler<ProcessLeagueKickoutsCommand, Result<int>> ProcessLeagueKickouts { get; }
    ICommandHandler<InitializeLeagueTournamentCommand, Result> InitializeLeagueTournament { get; }
    ICommandHandler<LeaveFantasyLeagueCommand, Result> LeaveFantasyLeague { get; }

    // Achievements
    ICommandHandler<CreateAchievementCommand, Result<Guid>> CreateAchievement { get; }
    ICommandHandler<UpdateAchievementCommand, Result> UpdateAchievement { get; }
    ICommandHandler<DeleteAchievementCommand, Result> DeleteAchievement { get; }
    ICommandHandler<UnlockUserAchievementCommand, Result<Guid>> UnlockUserAchievement { get; }

    // Country
    ICommandHandler<CreateCountryCommand, Result<Guid>> CreateCountry { get; }
    ICommandHandler<UpdateCountryCommand, Result> UpdateCountry { get; }
    ICommandHandler<DeleteCountryCommand, Result> DeleteCountry { get; }

    // Venue
    ICommandHandler<CreateVenueCommand, Result<Guid>> CreateVenue { get; }
    ICommandHandler<UpdateVenueCommand, Result> UpdateVenue { get; }
    ICommandHandler<DeleteVenueCommand, Result> DeleteVenue { get; }

    // League
    ICommandHandler<CreateLeagueCommand, Result<Guid>> CreateLeague { get; }
    ICommandHandler<UpdateLeagueCommand, Result> UpdateLeague { get; }
    ICommandHandler<DeleteLeagueCommand, Result> DeleteLeague { get; }

    // Team
    ICommandHandler<CreateTeamCommand, Result<Guid>> CreateTeam { get; }
    ICommandHandler<UpdateTeamCommand, Result> UpdateTeam { get; }
    ICommandHandler<DeleteTeamCommand, Result> DeleteTeam { get; }

    // Player
    ICommandHandler<CreatePlayerCommand, Result<Guid>> CreatePlayer { get; }
    ICommandHandler<UpdatePlayerCommand, Result> UpdatePlayer { get; }
    ICommandHandler<DeletePlayerCommand, Result> DeletePlayer { get; }

    // Fixture
    ICommandHandler<CreateFixtureCommand, Result<Guid>> CreateFixture { get; }
    ICommandHandler<UpdateFixtureCommand, Result> UpdateFixture { get; }
    ICommandHandler<UpdateFixtureScoreCommand, Result> UpdateFixtureScore { get; }
    ICommandHandler<DeleteFixtureCommand, Result> DeleteFixture { get; }

    // Coach
    ICommandHandler<CreateCoachCommand, Result<Guid>> CreateCoach { get; }
    ICommandHandler<UpdateCoachCommand, Result> UpdateCoach { get; }
    ICommandHandler<DeleteCoachCommand, Result> DeleteCoach { get; }

    // PlayerMatchStat
    ICommandHandler<CreatePlayerMatchStatCommand, Result<Guid>> CreatePlayerMatchStat { get; }
    ICommandHandler<UpdatePlayerMatchStatCommand, Result> UpdatePlayerMatchStat { get; }
    ICommandHandler<DeletePlayerMatchStatCommand, Result> DeletePlayerMatchStat { get; }

    // Predict Club
    ICommandHandler<CreatePredictClubCommand, Result<Guid>> CreatePredictClub { get; }
    ICommandHandler<JoinPredictClubCommand, Result> JoinPredictClub { get; }
    ICommandHandler<SaveMatchPredictionCommand, Result> SaveMatchPrediction { get; }
    ICommandHandler<SaveTournamentPredictionCommand, Result> SaveTournamentPrediction { get; }

    // Extended Football Domains
    ICommandHandler<CreateBookmakerCommand, Result<Guid>> CreateBookmaker { get; }
    ICommandHandler<CreateFixtureOddCommand, Result<Guid>> CreateFixtureOdd { get; }
    ICommandHandler<CreatePlayerTransferCommand, Result<Guid>> CreatePlayerTransfer { get; }
    ICommandHandler<CreatePlayerInjuryCommand, Result<Guid>> CreatePlayerInjury { get; }
}

public sealed class FootballFantasyCommandUnit : IFootballFantasyCommandUnit
{
    public ICommandHandler<CreateFantasyLeagueCommand, Result<Guid>> CreateFantasyLeague { get; }
    public ICommandHandler<CreateFantasyTeamCommand, Result<Guid>> CreateFantasyTeam { get; }
    public ICommandHandler<SaveLineupCommand, Result> SaveLineup { get; }
    public ICommandHandler<ReviewJoinRequestCommand, Result> ReviewJoinRequest { get; }
    public ICommandHandler<WithdrawJoinRequestCommand, Result> WithdrawJoinRequest { get; }
    public ICommandHandler<ProcessLeagueKickoutsCommand, Result<int>> ProcessLeagueKickouts { get; }
    public ICommandHandler<InitializeLeagueTournamentCommand, Result> InitializeLeagueTournament { get; }
    public ICommandHandler<LeaveFantasyLeagueCommand, Result> LeaveFantasyLeague { get; }

    public ICommandHandler<CreatePredictClubCommand, Result<Guid>> CreatePredictClub { get; }
    public ICommandHandler<JoinPredictClubCommand, Result> JoinPredictClub { get; }
    public ICommandHandler<SaveMatchPredictionCommand, Result> SaveMatchPrediction { get; }
    public ICommandHandler<SaveTournamentPredictionCommand, Result> SaveTournamentPrediction { get; }

    public ICommandHandler<CreateAchievementCommand, Result<Guid>> CreateAchievement { get; }
    public ICommandHandler<UpdateAchievementCommand, Result> UpdateAchievement { get; }
    public ICommandHandler<DeleteAchievementCommand, Result> DeleteAchievement { get; }
    public ICommandHandler<UnlockUserAchievementCommand, Result<Guid>> UnlockUserAchievement { get; }

    public ICommandHandler<CreateCountryCommand, Result<Guid>> CreateCountry { get; }
    public ICommandHandler<UpdateCountryCommand, Result> UpdateCountry { get; }
    public ICommandHandler<DeleteCountryCommand, Result> DeleteCountry { get; }

    public ICommandHandler<CreateVenueCommand, Result<Guid>> CreateVenue { get; }
    public ICommandHandler<UpdateVenueCommand, Result> UpdateVenue { get; }
    public ICommandHandler<DeleteVenueCommand, Result> DeleteVenue { get; }

    public ICommandHandler<CreateLeagueCommand, Result<Guid>> CreateLeague { get; }
    public ICommandHandler<UpdateLeagueCommand, Result> UpdateLeague { get; }
    public ICommandHandler<DeleteLeagueCommand, Result> DeleteLeague { get; }

    public ICommandHandler<CreateTeamCommand, Result<Guid>> CreateTeam { get; }
    public ICommandHandler<UpdateTeamCommand, Result> UpdateTeam { get; }
    public ICommandHandler<DeleteTeamCommand, Result> DeleteTeam { get; }

    public ICommandHandler<CreatePlayerCommand, Result<Guid>> CreatePlayer { get; }
    public ICommandHandler<UpdatePlayerCommand, Result> UpdatePlayer { get; }
    public ICommandHandler<DeletePlayerCommand, Result> DeletePlayer { get; }

    public ICommandHandler<CreateFixtureCommand, Result<Guid>> CreateFixture { get; }
    public ICommandHandler<UpdateFixtureCommand, Result> UpdateFixture { get; }
    public ICommandHandler<UpdateFixtureScoreCommand, Result> UpdateFixtureScore { get; }
    public ICommandHandler<DeleteFixtureCommand, Result> DeleteFixture { get; }

    public ICommandHandler<CreateCoachCommand, Result<Guid>> CreateCoach { get; }
    public ICommandHandler<UpdateCoachCommand, Result> UpdateCoach { get; }
    public ICommandHandler<DeleteCoachCommand, Result> DeleteCoach { get; }

    public ICommandHandler<CreatePlayerMatchStatCommand, Result<Guid>> CreatePlayerMatchStat { get; }
    public ICommandHandler<UpdatePlayerMatchStatCommand, Result> UpdatePlayerMatchStat { get; }
    public ICommandHandler<DeletePlayerMatchStatCommand, Result> DeletePlayerMatchStat { get; }

    public FootballFantasyCommandUnit(
        ICommandHandler<CreateFantasyLeagueCommand, Result<Guid>> createFantasyLeague,
        ICommandHandler<CreateFantasyTeamCommand, Result<Guid>> createFantasyTeam,
        ICommandHandler<SaveLineupCommand, Result> saveLineup,
        ICommandHandler<ReviewJoinRequestCommand, Result> reviewJoinRequest,
        ICommandHandler<WithdrawJoinRequestCommand, Result> withdrawJoinRequest,
        ICommandHandler<ProcessLeagueKickoutsCommand, Result<int>> processLeagueKickouts,
        ICommandHandler<InitializeLeagueTournamentCommand, Result> initializeLeagueTournament,
        ICommandHandler<LeaveFantasyLeagueCommand, Result> leaveFantasyLeague,
        ICommandHandler<CreateAchievementCommand, Result<Guid>> createAchievement,
        ICommandHandler<UpdateAchievementCommand, Result> updateAchievement,
        ICommandHandler<DeleteAchievementCommand, Result> deleteAchievement,
        ICommandHandler<UnlockUserAchievementCommand, Result<Guid>> unlockUserAchievement,
        ICommandHandler<CreateCountryCommand, Result<Guid>> createCountry,
        ICommandHandler<UpdateCountryCommand, Result> updateCountry,
        ICommandHandler<DeleteCountryCommand, Result> deleteCountry,
        ICommandHandler<CreateVenueCommand, Result<Guid>> createVenue,
        ICommandHandler<UpdateVenueCommand, Result> updateVenue,
        ICommandHandler<DeleteVenueCommand, Result> deleteVenue,
        ICommandHandler<CreateLeagueCommand, Result<Guid>> createLeague,
        ICommandHandler<UpdateLeagueCommand, Result> updateLeague,
        ICommandHandler<DeleteLeagueCommand, Result> deleteLeague,
        ICommandHandler<CreateTeamCommand, Result<Guid>> createTeam,
        ICommandHandler<UpdateTeamCommand, Result> updateTeam,
        ICommandHandler<DeleteTeamCommand, Result> deleteTeam,
        ICommandHandler<CreatePlayerCommand, Result<Guid>> createPlayer,
        ICommandHandler<UpdatePlayerCommand, Result> updatePlayer,
        ICommandHandler<DeletePlayerCommand, Result> deletePlayer,
        ICommandHandler<CreateFixtureCommand, Result<Guid>> createFixture,
        ICommandHandler<UpdateFixtureCommand, Result> updateFixture,
        ICommandHandler<UpdateFixtureScoreCommand, Result> updateFixtureScore,
        ICommandHandler<DeleteFixtureCommand, Result> deleteFixture,
        ICommandHandler<CreateCoachCommand, Result<Guid>> createCoach,
        ICommandHandler<UpdateCoachCommand, Result> updateCoach,
        ICommandHandler<DeleteCoachCommand, Result> deleteCoach,
        ICommandHandler<CreatePlayerMatchStatCommand, Result<Guid>> createPlayerMatchStat,
        ICommandHandler<UpdatePlayerMatchStatCommand, Result> updatePlayerMatchStat,
        ICommandHandler<DeletePlayerMatchStatCommand, Result> deletePlayerMatchStat,
        ICommandHandler<CreatePredictClubCommand, Result<Guid>> createPredictClub,
        ICommandHandler<JoinPredictClubCommand, Result> joinPredictClub,
        ICommandHandler<SaveMatchPredictionCommand, Result> saveMatchPrediction,
        ICommandHandler<SaveTournamentPredictionCommand, Result> saveTournamentPrediction,
        ICommandHandler<CreateBookmakerCommand, Result<Guid>> createBookmaker,
        ICommandHandler<CreateFixtureOddCommand, Result<Guid>> createFixtureOdd,
        ICommandHandler<CreatePlayerTransferCommand, Result<Guid>> createPlayerTransfer,
        ICommandHandler<CreatePlayerInjuryCommand, Result<Guid>> createPlayerInjury)
    {
        CreateFantasyLeague = createFantasyLeague;
        CreateFantasyTeam = createFantasyTeam;
        SaveLineup = saveLineup;
        ReviewJoinRequest = reviewJoinRequest;
        WithdrawJoinRequest = withdrawJoinRequest;
        ProcessLeagueKickouts = processLeagueKickouts;
        InitializeLeagueTournament = initializeLeagueTournament;
        LeaveFantasyLeague = leaveFantasyLeague;
        CreatePredictClub = createPredictClub;
        JoinPredictClub = joinPredictClub;
        SaveMatchPrediction = saveMatchPrediction;
        SaveTournamentPrediction = saveTournamentPrediction;
        CreateAchievement = createAchievement;
        UpdateAchievement = updateAchievement;
        DeleteAchievement = deleteAchievement;
        UnlockUserAchievement = unlockUserAchievement;

        CreateCountry = createCountry;
        UpdateCountry = updateCountry;
        DeleteCountry = deleteCountry;

        CreateVenue = createVenue;
        UpdateVenue = updateVenue;
        DeleteVenue = deleteVenue;

        CreateLeague = createLeague;
        UpdateLeague = updateLeague;
        DeleteLeague = deleteLeague;

        CreateTeam = createTeam;
        UpdateTeam = updateTeam;
        DeleteTeam = deleteTeam;

        CreatePlayer = createPlayer;
        UpdatePlayer = updatePlayer;
        DeletePlayer = deletePlayer;

        CreateFixture = createFixture;
        UpdateFixture = updateFixture;
        UpdateFixtureScore = updateFixtureScore;
        DeleteFixture = deleteFixture;

        CreateCoach = createCoach;
        UpdateCoach = updateCoach;
        DeleteCoach = deleteCoach;

        CreatePlayerMatchStat = createPlayerMatchStat;
        UpdatePlayerMatchStat = updatePlayerMatchStat;
        DeletePlayerMatchStat = deletePlayerMatchStat;

        CreateBookmaker = createBookmaker;
        CreateFixtureOdd = createFixtureOdd;
        CreatePlayerTransfer = createPlayerTransfer;
        CreatePlayerInjury = createPlayerInjury;
    }

    public ICommandHandler<CreateBookmakerCommand, Result<Guid>> CreateBookmaker { get; }
    public ICommandHandler<CreateFixtureOddCommand, Result<Guid>> CreateFixtureOdd { get; }
    public ICommandHandler<CreatePlayerTransferCommand, Result<Guid>> CreatePlayerTransfer { get; }
    public ICommandHandler<CreatePlayerInjuryCommand, Result<Guid>> CreatePlayerInjury { get; }
}
