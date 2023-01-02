using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagementSystem.Server.Persistence.Migrations
{
    /// <inheritdoc />
    [Open]
    public partial class AddProductDepricatedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Name",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockCount2",
                table: "Products");

            migrationBuilder.AddColumn<bool>(
                name: "IsDepricated",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name_IsDepricated",
                table: "Products",
                columns: new[] { "Name", "IsDepricated" })
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Name_IsDepricated",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDepricated",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "StockCount2",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }
    }
}
