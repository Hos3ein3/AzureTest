using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureTest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandHabitTrackerAndLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HabitLogs_HabitTrackerId_LogDate",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "BooleanValue",
                table: "HabitLogs");

            migrationBuilder.RenameColumn(
                name: "Goal",
                table: "HabitTrackers",
                newName: "Target");

            migrationBuilder.RenameColumn(
                name: "LogDate",
                table: "HabitLogs",
                newName: "LoggedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "HabitTrackers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Direction",
                table: "HabitTrackers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "HabitTrackers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "HabitTrackers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Schedule",
                table: "HabitTrackers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "HabitTrackers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "UTC");

            migrationBuilder.AddColumn<string>(
                name: "TypeConfig",
                table: "HabitTrackers",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "NumericValue",
                table: "HabitLogs",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "HabitLogs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LocalDate",
                table: "HabitLogs",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<double>(
                name: "TargetSnapshot",
                table: "HabitLogs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "HabitLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "HabitLogs",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE \"HabitLogs\" SET \"LocalDate\" = (\"LoggedAt\"::date) WHERE \"LocalDate\" = '0001-01-01';");
            migrationBuilder.Sql("UPDATE \"HabitLogs\" l SET \"UserId\" = t.\"UserId\" FROM \"HabitTrackers\" t WHERE l.\"HabitTrackerId\" = t.\"Id\" AND l.\"UserId\" = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.CreateIndex(
                name: "IX_HabitTrackers_UserId_IsArchived",
                table: "HabitTrackers",
                columns: new[] { "UserId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_HabitLogs_HabitTrackerId_LocalDate",
                table: "HabitLogs",
                columns: new[] { "HabitTrackerId", "LocalDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HabitLogs_UserId_LocalDate",
                table: "HabitLogs",
                columns: new[] { "UserId", "LocalDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HabitTrackers_UserId_IsArchived",
                table: "HabitTrackers");

            migrationBuilder.DropIndex(
                name: "IX_HabitLogs_HabitTrackerId_LocalDate",
                table: "HabitLogs");

            migrationBuilder.DropIndex(
                name: "IX_HabitLogs_UserId_LocalDate",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "HabitTrackers");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "HabitTrackers");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "HabitTrackers");

            migrationBuilder.DropColumn(
                name: "Schedule",
                table: "HabitTrackers");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "HabitTrackers");

            migrationBuilder.DropColumn(
                name: "TypeConfig",
                table: "HabitTrackers");

            migrationBuilder.DropColumn(
                name: "LocalDate",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "TargetSnapshot",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "HabitLogs");

            migrationBuilder.RenameColumn(
                name: "Target",
                table: "HabitTrackers",
                newName: "Goal");

            migrationBuilder.RenameColumn(
                name: "LoggedAt",
                table: "HabitLogs",
                newName: "LogDate");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "HabitTrackers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "NumericValue",
                table: "HabitLogs",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "HabitLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BooleanValue",
                table: "HabitLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_HabitLogs_HabitTrackerId_LogDate",
                table: "HabitLogs",
                columns: new[] { "HabitTrackerId", "LogDate" },
                unique: true);
        }
    }
}
