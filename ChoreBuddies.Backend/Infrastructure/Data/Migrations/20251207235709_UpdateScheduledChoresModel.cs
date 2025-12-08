using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class UpdateScheduledChoresModel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AssignedTo",
            table: "ScheduledChores");

        migrationBuilder.AddColumn<int>(
            name: "UserId",
            table: "ScheduledChores",
            type: "int",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "UserId",
            table: "ScheduledChores");

        migrationBuilder.AddColumn<string>(
            name: "AssignedTo",
            table: "ScheduledChores",
            type: "nvarchar(max)",
            nullable: true);
    }
}
