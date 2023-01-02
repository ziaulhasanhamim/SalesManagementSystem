using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagementSystem.Server.Persistence.Migrations
{
    /// <inheritdoc />
    [Open]
    public partial class FixFieldTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDepricated",
                table: "Products",
                newName: "IsDeprecated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDeprecated",
                table: "Products",
                newName: "IsDepricated");
        }
    }
}
