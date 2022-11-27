using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagementSystem.Server.Persistence.Migrations
{
    /// <inheritdoc />
    [Open]
    public partial class AddDeleteCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesEntries_Customers_CustomerId",
                table: "SalesEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesEntries_Customers_CustomerId",
                table: "SalesEntries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesEntries_Customers_CustomerId",
                table: "SalesEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesEntries_Customers_CustomerId",
                table: "SalesEntries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }
    }
}
