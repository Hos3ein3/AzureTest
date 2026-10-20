using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureTest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConnectPomodoroToTaskAndNullableHabitTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TaskItemId",
                table: "PomodoroSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Target",
                table: "HabitTrackers",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_TaskItemId",
                table: "PomodoroSessions",
                column: "TaskItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_PomodoroSessions_Tasks_TaskItemId",
                table: "PomodoroSessions",
                column: "TaskItemId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PomodoroSessions_Tasks_TaskItemId",
                table: "PomodoroSessions");

            migrationBuilder.DropIndex(
                name: "IX_PomodoroSessions_TaskItemId",
                table: "PomodoroSessions");

            migrationBuilder.DropColumn(
                name: "TaskItemId",
                table: "PomodoroSessions");

            migrationBuilder.AlterColumn<double>(
                name: "Target",
                table: "HabitTrackers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);
        }
    }
}
