using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureTest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Tickets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "TicketMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Resumes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PomodoroSettings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PomodoroSessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "HabitTrackers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "HabitLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Expenses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Events",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Debts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TenantId",
                table: "Tickets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TenantId",
                table: "Tasks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_TenantId",
                table: "Resumes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSettings_TenantId",
                table: "PomodoroSettings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_TenantId",
                table: "PomodoroSessions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_HabitTrackers_TenantId",
                table: "HabitTrackers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_TenantId",
                table: "Expenses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TenantId",
                table: "Events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Debts_TenantId",
                table: "Debts",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_TenantId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_TenantId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Resumes_TenantId",
                table: "Resumes");

            migrationBuilder.DropIndex(
                name: "IX_PomodoroSettings_TenantId",
                table: "PomodoroSettings");

            migrationBuilder.DropIndex(
                name: "IX_PomodoroSessions_TenantId",
                table: "PomodoroSessions");

            migrationBuilder.DropIndex(
                name: "IX_HabitTrackers_TenantId",
                table: "HabitTrackers");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_TenantId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Events_TenantId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Debts_TenantId",
                table: "Debts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TicketMessages");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PomodoroSettings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PomodoroSessions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "HabitTrackers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Debts");
        }
    }
}
