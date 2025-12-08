using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddMissingRelationsUser : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Households_AspNetUsers_OwnerId",
            table: "Households");

        migrationBuilder.DropIndex(
            name: "IX_Households_OwnerId",
            table: "Households");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Households_OwnerId",
            table: "Households",
            column: "OwnerId");

        migrationBuilder.AddForeignKey(
            name: "FK_Households_AspNetUsers_OwnerId",
            table: "Households",
            column: "OwnerId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
