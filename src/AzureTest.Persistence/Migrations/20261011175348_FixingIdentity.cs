using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AzureTest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixingIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "FootballFantasy");

            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.CreateTable(
                name: "Achievements",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BadgeIcon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PointsReward = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BetTypes",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bookmakers",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmakers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Continent = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FlagUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Debts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    DateOfBorrow = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateOfReturn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PromissedReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Profit = table.Column<decimal>(type: "numeric", nullable: false),
                    DebtPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsDone = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Debts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAllDay = table.Column<bool>(type: "boolean", nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HabitTrackers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    Goal = table.Column<double>(type: "double precision", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitTrackers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AreaName = table.Column<string>(type: "text", nullable: true),
                    ControllerName = table.Column<string>(type: "text", nullable: false),
                    ActionName = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsRestricted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PredictClubs",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClubTag = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Format = table.Column<int>(type: "integer", nullable: false),
                    ScopeType = table.Column<int>(type: "integer", nullable: false),
                    ScopeValue = table.Column<string>(type: "text", nullable: false),
                    MaxMembers = table.Column<int>(type: "integer", nullable: true),
                    JoinCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExactScorePoints = table.Column<int>(type: "integer", nullable: false),
                    OutcomePoints = table.Column<int>(type: "integer", nullable: false),
                    HalfTimeScorePoints = table.Column<int>(type: "integer", nullable: false),
                    BothTeamsToScorePoints = table.Column<int>(type: "integer", nullable: false),
                    OverUnderPoints = table.Column<int>(type: "integer", nullable: false),
                    YellowCardsPoints = table.Column<int>(type: "integer", nullable: false),
                    CornersPoints = table.Column<int>(type: "integer", nullable: false),
                    MatchMvpPoints = table.Column<int>(type: "integer", nullable: false),
                    FirstGoalMinutePoints = table.Column<int>(type: "integer", nullable: false),
                    EnableTournamentPredictions = table.Column<bool>(type: "boolean", nullable: false),
                    TournamentPodiumPoints = table.Column<int>(type: "integer", nullable: false),
                    TournamentAwardPoints = table.Column<int>(type: "integer", nullable: false),
                    PredictExactScore = table.Column<bool>(type: "boolean", nullable: false),
                    PredictOutcome = table.Column<bool>(type: "boolean", nullable: false),
                    PredictHalfTime = table.Column<bool>(type: "boolean", nullable: false),
                    PredictBtts = table.Column<bool>(type: "boolean", nullable: false),
                    PredictCards = table.Column<bool>(type: "boolean", nullable: false),
                    PredictCorners = table.Column<bool>(type: "boolean", nullable: false),
                    PredictMvp = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictClubs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resumes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JobLink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Note = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AtsScore = table.Column<int>(type: "integer", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    ChatUrl = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resumes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScoringRuleSets",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringRuleSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileImage = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Coaches",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coaches_Countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leagues",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Continent = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leagues_Countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    PreferredFoot = table.Column<int>(type: "integer", nullable: true),
                    HeightCm = table.Column<int>(type: "integer", nullable: true),
                    WeightKg = table.Column<int>(type: "integer", nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Surface = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Venues_Countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HabitLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HabitTrackerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LogDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BooleanValue = table.Column<bool>(type: "boolean", nullable: false),
                    NumericValue = table.Column<double>(type: "double precision", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HabitLogs_HabitTrackers_HabitTrackerId",
                        column: x => x.HabitTrackerId,
                        principalTable: "HabitTrackers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "text", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "Identity",
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PredictClubMembers",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictClubId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPoints = table.Column<int>(type: "integer", nullable: false),
                    CorrectExactScores = table.Column<int>(type: "integer", nullable: false),
                    CorrectOutcomes = table.Column<int>(type: "integer", nullable: false),
                    TotalPredictionsCount = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictClubMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PredictClubMembers_PredictClubs_PredictClubId",
                        column: x => x.PredictClubId,
                        principalSchema: "FootballFantasy",
                        principalTable: "PredictClubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyLeagues",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LeagueTag = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    Format = table.Column<int>(type: "integer", nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    SquadMode = table.Column<int>(type: "integer", nullable: false),
                    SquadSize = table.Column<int>(type: "integer", nullable: false),
                    StartingSize = table.Column<int>(type: "integer", nullable: false),
                    MinSquadSize = table.Column<int>(type: "integer", nullable: false),
                    BenchSize = table.Column<int>(type: "integer", nullable: false),
                    Budget = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxPlayersPerRealTeam = table.Column<int>(type: "integer", nullable: true),
                    AllowedFormations = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CaptainEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CaptainMultiplier = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    AllowCoach = table.Column<bool>(type: "boolean", nullable: false),
                    AllowAutoSub = table.Column<bool>(type: "boolean", nullable: false),
                    EnableMvpBonus = table.Column<bool>(type: "boolean", nullable: false),
                    MvpBonusPoints = table.Column<int>(type: "integer", nullable: false),
                    FreeTransfersPerGameweek = table.Column<int>(type: "integer", nullable: false),
                    ExtraTransferPointPenalty = table.Column<int>(type: "integer", nullable: true),
                    ProfitTaxPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    GroupCount = table.Column<int>(type: "integer", nullable: true),
                    MaxMembers = table.Column<int>(type: "integer", nullable: true),
                    JoinCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    KickoutType = table.Column<int>(type: "integer", nullable: false),
                    AuctionStartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuctionEndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScoringRuleSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CloseTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Window1Open = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Window1Close = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Window2Open = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Window2Close = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyLeagues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyLeagues_ScoringRuleSets_ScoringRuleSetId",
                        column: x => x.ScoringRuleSetId,
                        principalSchema: "FootballFantasy",
                        principalTable: "ScoringRuleSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScoringRules",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Metric = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Threshold = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoringRules_ScoringRuleSets_RuleSetId",
                        column: x => x.RuleSetId,
                        principalSchema: "FootballFantasy",
                        principalTable: "ScoringRuleSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsReadByCreator = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketMessages_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    JwtId = table.Column<string>(type: "text", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Used = table.Column<bool>(type: "boolean", nullable: false),
                    Invalidated = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                schema: "Identity",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerProfiles",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Biography = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Agent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MarketValueEur = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ContractUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SocialMediaHandle = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerProfiles_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sidelined",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sidelined", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sidelined_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Sidelined_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TeamType = table.Column<int>(type: "integer", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: true),
                    FoundedYear = table.Column<int>(type: "integer", nullable: true),
                    StadiumName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    StadiumCapacity = table.Column<int>(type: "integer", nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Teams_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FantasyLeagueGroups",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GroupNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyLeagueGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyLeagueGroups_FantasyLeagues_FantasyLeagueId",
                        column: x => x.FantasyLeagueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyLeagueInvites",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyLeagueInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyLeagueInvites_FantasyLeagues_FantasyLeagueId",
                        column: x => x.FantasyLeagueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyLeagueMembers",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyLeagueMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyLeagueMembers_FantasyLeagues_FantasyLeagueId",
                        column: x => x.FantasyLeagueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyLeaguePoolRules",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<int>(type: "integer", nullable: false),
                    ScopeValue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsExclude = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyLeaguePoolRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyLeaguePoolRules_FantasyLeagues_FantasyLeagueId",
                        column: x => x.FantasyLeagueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyLeaguePositionLimits",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    MinInSquad = table.Column<int>(type: "integer", nullable: false),
                    MaxInSquad = table.Column<int>(type: "integer", nullable: false),
                    MinInStarting = table.Column<int>(type: "integer", nullable: false),
                    MaxInStarting = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyLeaguePositionLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyLeaguePositionLimits_FantasyLeagues_FantasyLeagueId",
                        column: x => x.FantasyLeagueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyPeriods",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LineupDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyPeriods_FantasyLeagues_FantasyLeagueId",
                        column: x => x.FantasyLeagueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyTeams",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreditsLeft = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalPoints = table.Column<int>(type: "integer", nullable: false),
                    GameweekTransfersMade = table.Column<int>(type: "integer", nullable: false),
                    IsFinalized = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsSpectator = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyTeams_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FantasyTeams_FantasyLeagues_FantasyLeagueId",
                        column: x => x.FantasyLeagueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyLeagueSources",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyLeagueSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyLeagueSources_FantasyLeagues_FantasyLeagueId",
                        column: x => x.FantasyLeagueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FantasyLeagueSources_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerPrices",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerPrices_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerPrices_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stages",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stages_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HeadToHeads",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Team1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Team2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalPlayed = table.Column<int>(type: "integer", nullable: false),
                    Team1Wins = table.Column<int>(type: "integer", nullable: false),
                    Team2Wins = table.Column<int>(type: "integer", nullable: false),
                    Draws = table.Column<int>(type: "integer", nullable: false),
                    Team1Goals = table.Column<int>(type: "integer", nullable: false),
                    Team2Goals = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeadToHeads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeadToHeads_Teams_Team1Id",
                        column: x => x.Team1Id,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HeadToHeads_Teams_Team2Id",
                        column: x => x.Team2Id,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeagueStandings",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    GoalsDiff = table.Column<int>(type: "integer", nullable: false),
                    Form = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Played = table.Column<int>(type: "integer", nullable: false),
                    Won = table.Column<int>(type: "integer", nullable: false),
                    Drawn = table.Column<int>(type: "integer", nullable: false),
                    Lost = table.Column<int>(type: "integer", nullable: false),
                    GoalsFor = table.Column<int>(type: "integer", nullable: false),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueStandings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueStandings_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeagueStandings_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSeasonStatistics",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Appearances = table.Column<int>(type: "integer", nullable: false),
                    Lineups = table.Column<int>(type: "integer", nullable: false),
                    MinutesPlayed = table.Column<int>(type: "integer", nullable: false),
                    Goals = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    YellowCards = table.Column<int>(type: "integer", nullable: false),
                    RedCards = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSeasonStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonStatistics_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonStatistics_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonStatistics_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerTeamHistories",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JerseyNumber = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerTeamHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerTeamHistories_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerTeamHistories_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerTopScorers",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Goals = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: true),
                    PenaltyGoals = table.Column<int>(type: "integer", nullable: true),
                    MatchesPlayed = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerTopScorers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerTopScorers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerTopScorers_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerTopScorers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerTransfers",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Fee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerTransfers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerTransfers_Teams_FromTeamId",
                        column: x => x.FromTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerTransfers_Teams_ToTeamId",
                        column: x => x.ToTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamCoaches",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamCoaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamCoaches_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamCoaches_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamSeasons",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamSeasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamSeasons_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamSeasons_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamStatistics",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchesPlayed = table.Column<int>(type: "integer", nullable: false),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    Draws = table.Column<int>(type: "integer", nullable: false),
                    Losses = table.Column<int>(type: "integer", nullable: false),
                    GoalsFor = table.Column<int>(type: "integer", nullable: false),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false),
                    CleanSheets = table.Column<int>(type: "integer", nullable: false),
                    FailedToScore = table.Column<int>(type: "integer", nullable: false),
                    YellowCards = table.Column<int>(type: "integer", nullable: false),
                    RedCards = table.Column<int>(type: "integer", nullable: false),
                    Form = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamStatistics_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamStatistics_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamTrophies",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeagueName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Season = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Place = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamTrophies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamTrophies_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TeamTrophies_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TeamTrophies_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TournamentPredictions",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictClubId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FirstPlaceTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecondPlaceTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    ThirdPlaceTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    BestPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TopScorerPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TopAssisterPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false),
                    IsScored = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentPredictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentPredictions_Players_BestPlayerId",
                        column: x => x.BestPlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TournamentPredictions_Players_TopAssisterPlayerId",
                        column: x => x.TopAssisterPlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TournamentPredictions_Players_TopScorerPlayerId",
                        column: x => x.TopScorerPlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TournamentPredictions_PredictClubs_PredictClubId",
                        column: x => x.PredictClubId,
                        principalSchema: "FootballFantasy",
                        principalTable: "PredictClubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentPredictions_Teams_FirstPlaceTeamId",
                        column: x => x.FirstPlaceTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TournamentPredictions_Teams_SecondPlaceTeamId",
                        column: x => x.SecondPlaceTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TournamentPredictions_Teams_ThirdPlaceTeamId",
                        column: x => x.ThirdPlaceTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FantasyLineups",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Formation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CaptainPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ViceCaptainPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyLineups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyLineups_FantasyPeriods_FantasyPeriodId",
                        column: x => x.FantasyPeriodId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FantasyLineups_FantasyTeams_FantasyTeamId",
                        column: x => x.FantasyTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FantasyLineups_Players_CaptainPlayerId",
                        column: x => x.CaptainPlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FantasyLineups_Players_ViceCaptainPlayerId",
                        column: x => x.ViceCaptainPlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FantasyMatchups",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLeagueGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoundNumber = table.Column<int>(type: "integer", nullable: false),
                    StageName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MatchNumber = table.Column<int>(type: "integer", nullable: false),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    HomeScore = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: false),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    AwayScore = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: false),
                    WinnerTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyMatchups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyMatchups_FantasyLeagueGroups_FantasyLeagueGroupId",
                        column: x => x.FantasyLeagueGroupId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLeagueGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FantasyMatchups_FantasyLeagues_FantasyLeagueId",
                        column: x => x.FantasyLeagueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FantasyMatchups_FantasyPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FantasyMatchups_FantasyTeams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FantasyMatchups_FantasyTeams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FantasyMatchups_FantasyTeams_WinnerTeamId",
                        column: x => x.WinnerTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FantasySquadEntries",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PricePaid = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasySquadEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasySquadEntries_FantasyTeams_FantasyTeamId",
                        column: x => x.FantasyTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FantasySquadEntries_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FantasyTeamChips",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChipType = table.Column<int>(type: "integer", nullable: false),
                    UsedInPeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyTeamChips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyTeamChips_FantasyTeams_FantasyTeamId",
                        column: x => x.FantasyTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FantasyTeamPeriodScores",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyTeamPeriodScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyTeamPeriodScores_FantasyPeriods_FantasyPeriodId",
                        column: x => x.FantasyPeriodId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FantasyTeamPeriodScores_FantasyTeams_FantasyTeamId",
                        column: x => x.FantasyTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fixtures",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageId = table.Column<Guid>(type: "uuid", nullable: true),
                    Round = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoundNumber = table.Column<int>(type: "integer", nullable: true),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    KickoffAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Minute = table.Column<int>(type: "integer", nullable: true),
                    HomeScore = table.Column<int>(type: "integer", nullable: true),
                    AwayScore = table.Column<int>(type: "integer", nullable: true),
                    HomeScorePen = table.Column<int>(type: "integer", nullable: true),
                    AwayScorePen = table.Column<int>(type: "integer", nullable: true),
                    Venue = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Referee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsStatsFinalized = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fixtures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fixtures_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fixtures_Stages_StageId",
                        column: x => x.StageId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fixtures_Teams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fixtures_Teams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSquads",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamSeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShirtNumber = table.Column<int>(type: "integer", nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSquads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSquads_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerSquads_TeamSeasons_TeamSeasonId",
                        column: x => x.TeamSeasonId,
                        principalSchema: "FootballFantasy",
                        principalTable: "TeamSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FantasyLineupSlots",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLineupId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsStarter = table.Column<bool>(type: "boolean", nullable: false),
                    BenchOrder = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyLineupSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyLineupSlots_FantasyLineups_FantasyLineupId",
                        column: x => x.FantasyLineupId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLineups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FantasyLineupSlots_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FantasyPeriodFixtures",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    FixtureId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyPeriodFixtures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyPeriodFixtures_FantasyPeriods_FantasyPeriodId",
                        column: x => x.FantasyPeriodId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FantasyPeriodFixtures_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FantasyPlayerScores",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyLeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    FantasyPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FixtureId = table.Column<Guid>(type: "uuid", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Breakdown = table.Column<string>(type: "jsonb", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyPlayerScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyPlayerScores_FantasyLeagues_FantasyLeagueId",
                        column: x => x.FantasyLeagueId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FantasyPlayerScores_FantasyPeriods_FantasyPeriodId",
                        column: x => x.FantasyPeriodId,
                        principalSchema: "FootballFantasy",
                        principalTable: "FantasyPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FantasyPlayerScores_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FantasyPlayerScores_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FixtureEvents",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FixtureId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Minute = table.Column<int>(type: "integer", nullable: false),
                    ExtraMinute = table.Column<int>(type: "integer", nullable: true),
                    Detail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixtureEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixtureEvents_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixtureEvents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixtureEvents_Players_RelatedPlayerId",
                        column: x => x.RelatedPlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixtureEvents_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FixtureLineups",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FixtureId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: true),
                    Formation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixtureLineups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixtureLineups_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FixtureLineups_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FixtureLineups_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FixtureLiveUpdates",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FixtureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ElapsedMinute = table.Column<int>(type: "integer", nullable: false),
                    ExtraMinute = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: true),
                    AwayScore = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixtureLiveUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixtureLiveUpdates_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FixtureOdds",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FixtureId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookmakerId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketType = table.Column<int>(type: "integer", nullable: false),
                    ValueLabel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    IsLive = table.Column<bool>(type: "boolean", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixtureOdds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixtureOdds_Bookmakers_BookmakerId",
                        column: x => x.BookmakerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Bookmakers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixtureOdds_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FixtureStatistics",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FixtureId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixtureStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixtureStatistics_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FixtureStatistics_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MatchPredictions",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictClubId = table.Column<Guid>(type: "uuid", nullable: false),
                    FixtureId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PredictedHomeScore = table.Column<int>(type: "integer", nullable: true),
                    PredictedAwayScore = table.Column<int>(type: "integer", nullable: true),
                    PredictedHalfTimeHomeScore = table.Column<int>(type: "integer", nullable: true),
                    PredictedHalfTimeAwayScore = table.Column<int>(type: "integer", nullable: true),
                    BothTeamsToScore = table.Column<bool>(type: "boolean", nullable: true),
                    TotalGoalsOverUnder = table.Column<decimal>(type: "numeric", nullable: true),
                    PredictedTotalYellowCards = table.Column<int>(type: "integer", nullable: true),
                    PredictedTotalCorners = table.Column<int>(type: "integer", nullable: true),
                    PredictedFirstGoalMinute = table.Column<int>(type: "integer", nullable: true),
                    PredictedMvpPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false),
                    IsScored = table.Column<bool>(type: "boolean", nullable: false),
                    ScoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchPredictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchPredictions_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchPredictions_Players_PredictedMvpPlayerId",
                        column: x => x.PredictedMvpPlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MatchPredictions_PredictClubs_PredictClubId",
                        column: x => x.PredictClubId,
                        principalSchema: "FootballFantasy",
                        principalTable: "PredictClubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchStats",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FixtureId = table.Column<Guid>(type: "uuid", nullable: false),
                    MvpPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    HomePossession = table.Column<int>(type: "integer", nullable: false),
                    AwayPossession = table.Column<int>(type: "integer", nullable: false),
                    HomeShotsTotal = table.Column<int>(type: "integer", nullable: false),
                    AwayShotsTotal = table.Column<int>(type: "integer", nullable: false),
                    HomeShotsOnTarget = table.Column<int>(type: "integer", nullable: false),
                    AwayShotsOnTarget = table.Column<int>(type: "integer", nullable: false),
                    HomeCorners = table.Column<int>(type: "integer", nullable: false),
                    AwayCorners = table.Column<int>(type: "integer", nullable: false),
                    HomeFouls = table.Column<int>(type: "integer", nullable: false),
                    AwayFouls = table.Column<int>(type: "integer", nullable: false),
                    HomeYellowCards = table.Column<int>(type: "integer", nullable: false),
                    AwayYellowCards = table.Column<int>(type: "integer", nullable: false),
                    HomeRedCards = table.Column<int>(type: "integer", nullable: false),
                    AwayRedCards = table.Column<int>(type: "integer", nullable: false),
                    HomeOffsides = table.Column<int>(type: "integer", nullable: false),
                    AwayOffsides = table.Column<int>(type: "integer", nullable: false),
                    HomeHalfTimeScore = table.Column<int>(type: "integer", nullable: true),
                    AwayHalfTimeScore = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchStats_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchStats_Players_MvpPlayerId",
                        column: x => x.MvpPlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PlayerInjuries",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FixtureId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerInjuries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerInjuries_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlayerInjuries_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMatchStats",
                schema: "FootballFantasy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FixtureId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsStarter = table.Column<bool>(type: "boolean", nullable: false),
                    MinutesPlayed = table.Column<int>(type: "integer", nullable: false),
                    Goals = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    OwnGoals = table.Column<int>(type: "integer", nullable: false),
                    ShotsTotal = table.Column<int>(type: "integer", nullable: false),
                    ShotsOnTarget = table.Column<int>(type: "integer", nullable: false),
                    PassesTotal = table.Column<int>(type: "integer", nullable: false),
                    KeyPasses = table.Column<int>(type: "integer", nullable: false),
                    Tackles = table.Column<int>(type: "integer", nullable: false),
                    Interceptions = table.Column<int>(type: "integer", nullable: false),
                    Saves = table.Column<int>(type: "integer", nullable: false),
                    GoalsConceded = table.Column<int>(type: "integer", nullable: false),
                    PenaltiesSaved = table.Column<int>(type: "integer", nullable: false),
                    PenaltiesMissed = table.Column<int>(type: "integer", nullable: false),
                    YellowCards = table.Column<int>(type: "integer", nullable: false),
                    RedCards = table.Column<int>(type: "integer", nullable: false),
                    CleanSheet = table.Column<bool>(type: "boolean", nullable: false),
                    ProviderRating = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMatchStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStats_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerMatchStats_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "FootballFantasy",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_CountryId",
                schema: "FootballFantasy",
                table: "Coaches",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                schema: "FootballFantasy",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_UserId",
                table: "Events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLeagueGroups_FantasyLeagueId",
                schema: "FootballFantasy",
                table: "FantasyLeagueGroups",
                column: "FantasyLeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLeagueInvites_FantasyLeagueId",
                schema: "FootballFantasy",
                table: "FantasyLeagueInvites",
                column: "FantasyLeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLeagueInvites_Token",
                schema: "FootballFantasy",
                table: "FantasyLeagueInvites",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLeagueMembers_FantasyLeagueId_UserId",
                schema: "FootballFantasy",
                table: "FantasyLeagueMembers",
                columns: new[] { "FantasyLeagueId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLeaguePoolRules_FantasyLeagueId",
                schema: "FootballFantasy",
                table: "FantasyLeaguePoolRules",
                column: "FantasyLeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLeaguePositionLimits_FantasyLeagueId_Position",
                schema: "FootballFantasy",
                table: "FantasyLeaguePositionLimits",
                columns: new[] { "FantasyLeagueId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLeagues_LeagueTag",
                schema: "FootballFantasy",
                table: "FantasyLeagues",
                column: "LeagueTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLeagues_ScoringRuleSetId",
                schema: "FootballFantasy",
                table: "FantasyLeagues",
                column: "ScoringRuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLeagueSources_FantasyLeagueId_SeasonId",
                schema: "FootballFantasy",
                table: "FantasyLeagueSources",
                columns: new[] { "FantasyLeagueId", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLeagueSources_SeasonId",
                schema: "FootballFantasy",
                table: "FantasyLeagueSources",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLineups_CaptainPlayerId",
                schema: "FootballFantasy",
                table: "FantasyLineups",
                column: "CaptainPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLineups_FantasyPeriodId",
                schema: "FootballFantasy",
                table: "FantasyLineups",
                column: "FantasyPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLineups_FantasyTeamId_FantasyPeriodId",
                schema: "FootballFantasy",
                table: "FantasyLineups",
                columns: new[] { "FantasyTeamId", "FantasyPeriodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLineups_ViceCaptainPlayerId",
                schema: "FootballFantasy",
                table: "FantasyLineups",
                column: "ViceCaptainPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLineupSlots_FantasyLineupId_PlayerId",
                schema: "FootballFantasy",
                table: "FantasyLineupSlots",
                columns: new[] { "FantasyLineupId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyLineupSlots_PlayerId",
                schema: "FootballFantasy",
                table: "FantasyLineupSlots",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyMatchups_AwayTeamId",
                schema: "FootballFantasy",
                table: "FantasyMatchups",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyMatchups_FantasyLeagueGroupId",
                schema: "FootballFantasy",
                table: "FantasyMatchups",
                column: "FantasyLeagueGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyMatchups_FantasyLeagueId",
                schema: "FootballFantasy",
                table: "FantasyMatchups",
                column: "FantasyLeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyMatchups_HomeTeamId",
                schema: "FootballFantasy",
                table: "FantasyMatchups",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyMatchups_PeriodId",
                schema: "FootballFantasy",
                table: "FantasyMatchups",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyMatchups_WinnerTeamId",
                schema: "FootballFantasy",
                table: "FantasyMatchups",
                column: "WinnerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyPeriodFixtures_FantasyPeriodId_FixtureId",
                schema: "FootballFantasy",
                table: "FantasyPeriodFixtures",
                columns: new[] { "FantasyPeriodId", "FixtureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyPeriodFixtures_FixtureId",
                schema: "FootballFantasy",
                table: "FantasyPeriodFixtures",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyPeriods_FantasyLeagueId_Number",
                schema: "FootballFantasy",
                table: "FantasyPeriods",
                columns: new[] { "FantasyLeagueId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyPlayerScores_FantasyLeagueId_FixtureId_PlayerId",
                schema: "FootballFantasy",
                table: "FantasyPlayerScores",
                columns: new[] { "FantasyLeagueId", "FixtureId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyPlayerScores_FantasyPeriodId",
                schema: "FootballFantasy",
                table: "FantasyPlayerScores",
                column: "FantasyPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyPlayerScores_FixtureId",
                schema: "FootballFantasy",
                table: "FantasyPlayerScores",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyPlayerScores_PlayerId",
                schema: "FootballFantasy",
                table: "FantasyPlayerScores",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasySquadEntries_FantasyTeamId_PlayerId",
                schema: "FootballFantasy",
                table: "FantasySquadEntries",
                columns: new[] { "FantasyTeamId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasySquadEntries_PlayerId",
                schema: "FootballFantasy",
                table: "FantasySquadEntries",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeamChips_FantasyTeamId",
                schema: "FootballFantasy",
                table: "FantasyTeamChips",
                column: "FantasyTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeamPeriodScores_FantasyPeriodId",
                schema: "FootballFantasy",
                table: "FantasyTeamPeriodScores",
                column: "FantasyPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeamPeriodScores_FantasyTeamId_FantasyPeriodId",
                schema: "FootballFantasy",
                table: "FantasyTeamPeriodScores",
                columns: new[] { "FantasyTeamId", "FantasyPeriodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeams_CoachId",
                schema: "FootballFantasy",
                table: "FantasyTeams",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyTeams_FantasyLeagueId_UserId",
                schema: "FootballFantasy",
                table: "FantasyTeams",
                columns: new[] { "FantasyLeagueId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FixtureEvents_FixtureId",
                schema: "FootballFantasy",
                table: "FixtureEvents",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureEvents_PlayerId",
                schema: "FootballFantasy",
                table: "FixtureEvents",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureEvents_RelatedPlayerId",
                schema: "FootballFantasy",
                table: "FixtureEvents",
                column: "RelatedPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureEvents_TeamId",
                schema: "FootballFantasy",
                table: "FixtureEvents",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureLineups_CoachId",
                schema: "FootballFantasy",
                table: "FixtureLineups",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureLineups_FixtureId",
                schema: "FootballFantasy",
                table: "FixtureLineups",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureLineups_TeamId",
                schema: "FootballFantasy",
                table: "FixtureLineups",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureLiveUpdates_FixtureId",
                schema: "FootballFantasy",
                table: "FixtureLiveUpdates",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureOdds_BookmakerId",
                schema: "FootballFantasy",
                table: "FixtureOdds",
                column: "BookmakerId");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureOdds_FixtureId",
                schema: "FootballFantasy",
                table: "FixtureOdds",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_AwayTeamId",
                schema: "FootballFantasy",
                table: "Fixtures",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_HomeTeamId",
                schema: "FootballFantasy",
                table: "Fixtures",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_SeasonId",
                schema: "FootballFantasy",
                table: "Fixtures",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Fixtures_StageId",
                schema: "FootballFantasy",
                table: "Fixtures",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureStatistics_FixtureId",
                schema: "FootballFantasy",
                table: "FixtureStatistics",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_FixtureStatistics_TeamId",
                schema: "FootballFantasy",
                table: "FixtureStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HabitLogs_HabitTrackerId_LogDate",
                table: "HabitLogs",
                columns: new[] { "HabitTrackerId", "LogDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HabitTrackers_UserId",
                table: "HabitTrackers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeadToHeads_Team1Id",
                schema: "FootballFantasy",
                table: "HeadToHeads",
                column: "Team1Id");

            migrationBuilder.CreateIndex(
                name: "IX_HeadToHeads_Team2Id",
                schema: "FootballFantasy",
                table: "HeadToHeads",
                column: "Team2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_CountryId",
                schema: "FootballFantasy",
                table: "Leagues",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueStandings_SeasonId",
                schema: "FootballFantasy",
                table: "LeagueStandings",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueStandings_TeamId",
                schema: "FootballFantasy",
                table: "LeagueStandings",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPredictions_FixtureId",
                schema: "FootballFantasy",
                table: "MatchPredictions",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPredictions_PredictClubId_FixtureId_UserId",
                schema: "FootballFantasy",
                table: "MatchPredictions",
                columns: new[] { "PredictClubId", "FixtureId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchPredictions_PredictedMvpPlayerId",
                schema: "FootballFantasy",
                table: "MatchPredictions",
                column: "PredictedMvpPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchStats_FixtureId",
                schema: "FootballFantasy",
                table: "MatchStats",
                column: "FixtureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchStats_MvpPlayerId",
                schema: "FootballFantasy",
                table: "MatchStats",
                column: "MvpPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInjuries_FixtureId",
                schema: "FootballFantasy",
                table: "PlayerInjuries",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInjuries_PlayerId",
                schema: "FootballFantasy",
                table: "PlayerInjuries",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStats_FixtureId_PlayerId",
                schema: "FootballFantasy",
                table: "PlayerMatchStats",
                columns: new[] { "FixtureId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStats_PlayerId",
                schema: "FootballFantasy",
                table: "PlayerMatchStats",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMatchStats_TeamId",
                schema: "FootballFantasy",
                table: "PlayerMatchStats",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPrices_PlayerId",
                schema: "FootballFantasy",
                table: "PlayerPrices",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPrices_SeasonId",
                schema: "FootballFantasy",
                table: "PlayerPrices",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProfiles_PlayerId",
                schema: "FootballFantasy",
                table: "PlayerProfiles",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_CountryId",
                schema: "FootballFantasy",
                table: "Players",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonStatistics_PlayerId",
                schema: "FootballFantasy",
                table: "PlayerSeasonStatistics",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonStatistics_SeasonId",
                schema: "FootballFantasy",
                table: "PlayerSeasonStatistics",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonStatistics_TeamId",
                schema: "FootballFantasy",
                table: "PlayerSeasonStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSquads_PlayerId",
                schema: "FootballFantasy",
                table: "PlayerSquads",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSquads_TeamSeasonId",
                schema: "FootballFantasy",
                table: "PlayerSquads",
                column: "TeamSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTeamHistories_PlayerId",
                schema: "FootballFantasy",
                table: "PlayerTeamHistories",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTeamHistories_TeamId",
                schema: "FootballFantasy",
                table: "PlayerTeamHistories",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTopScorers_PlayerId",
                schema: "FootballFantasy",
                table: "PlayerTopScorers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTopScorers_SeasonId",
                schema: "FootballFantasy",
                table: "PlayerTopScorers",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTopScorers_TeamId",
                schema: "FootballFantasy",
                table: "PlayerTopScorers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTransfers_FromTeamId",
                schema: "FootballFantasy",
                table: "PlayerTransfers",
                column: "FromTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTransfers_PlayerId",
                schema: "FootballFantasy",
                table: "PlayerTransfers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTransfers_ToTeamId",
                schema: "FootballFantasy",
                table: "PlayerTransfers",
                column: "ToTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictClubMembers_PredictClubId_UserId",
                schema: "FootballFantasy",
                table: "PredictClubMembers",
                columns: new[] { "PredictClubId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PredictClubs_ClubTag",
                schema: "FootballFantasy",
                table: "PredictClubs",
                column: "ClubTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "Identity",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                schema: "Identity",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "Identity",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "Identity",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScoringRules_RuleSetId_Metric_Position",
                schema: "FootballFantasy",
                table: "ScoringRules",
                columns: new[] { "RuleSetId", "Metric", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_LeagueId_Name",
                schema: "FootballFantasy",
                table: "Seasons",
                columns: new[] { "LeagueId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sidelined_CoachId",
                schema: "FootballFantasy",
                table: "Sidelined",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_Sidelined_PlayerId",
                schema: "FootballFantasy",
                table: "Sidelined",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Stages_SeasonId",
                schema: "FootballFantasy",
                table: "Stages",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId",
                table: "Tasks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamCoaches_CoachId",
                schema: "FootballFantasy",
                table: "TeamCoaches",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamCoaches_TeamId",
                schema: "FootballFantasy",
                table: "TeamCoaches",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CountryId",
                schema: "FootballFantasy",
                table: "Teams",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_VenueId",
                schema: "FootballFantasy",
                table: "Teams",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSeasons_SeasonId",
                schema: "FootballFantasy",
                table: "TeamSeasons",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSeasons_TeamId_SeasonId",
                schema: "FootballFantasy",
                table: "TeamSeasons",
                columns: new[] { "TeamId", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_SeasonId",
                schema: "FootballFantasy",
                table: "TeamStatistics",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_TeamId",
                schema: "FootballFantasy",
                table: "TeamStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTrophies_CoachId",
                schema: "FootballFantasy",
                table: "TeamTrophies",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTrophies_PlayerId",
                schema: "FootballFantasy",
                table: "TeamTrophies",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTrophies_TeamId",
                schema: "FootballFantasy",
                table: "TeamTrophies",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_TicketId",
                table: "TicketMessages",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPredictions_BestPlayerId",
                schema: "FootballFantasy",
                table: "TournamentPredictions",
                column: "BestPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPredictions_FirstPlaceTeamId",
                schema: "FootballFantasy",
                table: "TournamentPredictions",
                column: "FirstPlaceTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPredictions_PredictClubId_UserId",
                schema: "FootballFantasy",
                table: "TournamentPredictions",
                columns: new[] { "PredictClubId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPredictions_SecondPlaceTeamId",
                schema: "FootballFantasy",
                table: "TournamentPredictions",
                column: "SecondPlaceTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPredictions_ThirdPlaceTeamId",
                schema: "FootballFantasy",
                table: "TournamentPredictions",
                column: "ThirdPlaceTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPredictions_TopAssisterPlayerId",
                schema: "FootballFantasy",
                table: "TournamentPredictions",
                column: "TopAssisterPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPredictions_TopScorerPlayerId",
                schema: "FootballFantasy",
                table: "TournamentPredictions",
                column: "TopScorerPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                schema: "FootballFantasy",
                table: "UserAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                schema: "FootballFantasy",
                table: "UserAchievements",
                columns: new[] { "UserId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                schema: "Identity",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                schema: "Identity",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "Identity",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "Identity",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "Identity",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venues_CountryId",
                schema: "FootballFantasy",
                table: "Venues",
                column: "CountryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BetTypes",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Debts");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "FantasyLeagueInvites",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyLeagueMembers",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyLeaguePoolRules",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyLeaguePositionLimits",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyLeagueSources",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyLineupSlots",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyMatchups",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyPeriodFixtures",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyPlayerScores",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasySquadEntries",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyTeamChips",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyTeamPeriodScores",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FixtureEvents",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FixtureLineups",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FixtureLiveUpdates",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FixtureOdds",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FixtureStatistics",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "HabitLogs");

            migrationBuilder.DropTable(
                name: "HeadToHeads",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "LeagueStandings",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "MatchPredictions",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "MatchStats",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "PlayerInjuries",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "PlayerMatchStats",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "PlayerPrices",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "PlayerProfiles",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "PlayerSeasonStatistics",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "PlayerSquads",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "PlayerTeamHistories",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "PlayerTopScorers",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "PlayerTransfers",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "PredictClubMembers",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Resumes");

            migrationBuilder.DropTable(
                name: "RoleClaims",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "ScoringRules",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Sidelined",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "TeamCoaches",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "TeamStatistics",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "TeamTrophies",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "TicketMessages");

            migrationBuilder.DropTable(
                name: "TournamentPredictions",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "UserAchievements",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "UserClaims",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserLogins",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserTokens",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "FantasyLineups",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyLeagueGroups",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Bookmakers",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "HabitTrackers");

            migrationBuilder.DropTable(
                name: "Fixtures",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "TeamSeasons",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "PredictClubs",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Achievements",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "FantasyPeriods",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyTeams",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Players",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Stages",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Teams",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Coaches",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "FantasyLeagues",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Seasons",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Venues",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "ScoringRuleSets",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Leagues",
                schema: "FootballFantasy");

            migrationBuilder.DropTable(
                name: "Countries",
                schema: "FootballFantasy");
        }
    }
}
