using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagementSystem.Server.Persistence.Migrations
{
    /// <inheritdoc />
    [Open]
    public partial class AddNewField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StockCount2",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockCount2",
                table: "Products");
        }
    }
}
