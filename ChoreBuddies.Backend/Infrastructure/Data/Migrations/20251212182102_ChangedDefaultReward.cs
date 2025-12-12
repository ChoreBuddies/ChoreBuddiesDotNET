using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class ChangedDefaultReward : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "SuggestedPoints",
            table: "DefaultRewards",
            newName: "QuantityAvailable");

        migrationBuilder.AddColumn<int>(
            name: "Cost",
            table: "DefaultRewards",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Cost",
            table: "DefaultRewards");

        migrationBuilder.RenameColumn(
            name: "QuantityAvailable",
            table: "DefaultRewards",
            newName: "SuggestedPoints");
    }
}
