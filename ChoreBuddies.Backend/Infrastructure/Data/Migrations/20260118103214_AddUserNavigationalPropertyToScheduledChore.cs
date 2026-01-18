using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddUserNavigationalPropertyToScheduledChore : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_ScheduledChores_UserId",
            table: "ScheduledChores",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_ScheduledChores_AspNetUsers_UserId",
            table: "ScheduledChores",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ScheduledChores_AspNetUsers_UserId",
            table: "ScheduledChores");

        migrationBuilder.DropIndex(
            name: "IX_ScheduledChores_UserId",
            table: "ScheduledChores");
    }
}
