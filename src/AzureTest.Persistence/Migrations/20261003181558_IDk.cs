using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureTest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IDk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "RolePermissions",
                schema: "identity",
                newName: "RolePermissions",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "Permissions",
                schema: "identity",
                newName: "Permissions",
                newSchema: "Identity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                schema: "Identity",
                newName: "RolePermissions",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "Permissions",
                schema: "Identity",
                newName: "Permissions",
                newSchema: "identity");
        }
    }
}
