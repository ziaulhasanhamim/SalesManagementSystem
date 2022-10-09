using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagementSystem.Server.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    TransactionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SalesGroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesEntries_SalesGroups_SalesGroupId",
                        column: x => x.SalesGroupId,
                        principalTable: "SalesGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesEntries_SalesGroupId",
                table: "SalesEntries",
                column: "SalesGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesEntries");

            migrationBuilder.DropTable(
                name: "SalesGroups");
        }
    }
}
