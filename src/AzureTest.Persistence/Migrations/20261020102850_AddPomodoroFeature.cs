using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureTest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPomodoroFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PomodoroSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SessionType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PlannedDurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    ActualDurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InterruptionCount = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UsedSkin = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_PomodoroSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PomodoroSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 25),
                    ShortBreakMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    LongBreakMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    LongBreakInterval = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    AutoStartBreaks = table.Column<bool>(type: "boolean", nullable: false),
                    AutoStartPomodoros = table.Column<bool>(type: "boolean", nullable: false),
                    SoundEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultSkin = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PomodoroSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_UserId",
                table: "PomodoroSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_UserId_StartedAtUtc",
                table: "PomodoroSessions",
                columns: new[] { "UserId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_UserId_Status",
                table: "PomodoroSessions",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSettings_UserId",
                table: "PomodoroSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PomodoroSessions");

            migrationBuilder.DropTable(
                name: "PomodoroSettings");
        }
    }
}
