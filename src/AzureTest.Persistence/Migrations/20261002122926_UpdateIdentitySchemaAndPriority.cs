using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureTest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentitySchemaAndPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "Identity",
                table: "Roles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "Identity",
                table: "Roles");
        }
    }
}
