using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class renameDefaultRewardToPredefinedReward : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DefaultRewards");

        migrationBuilder.CreateTable(
            name: "PredefinedRewards",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Cost = table.Column<int>(type: "int", nullable: false),
                QuantityAvailable = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PredefinedRewards", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PredefinedRewards");

        migrationBuilder.CreateTable(
            name: "DefaultRewards",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Cost = table.Column<int>(type: "int", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                QuantityAvailable = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DefaultRewards", x => x.Id);
            });
    }
}
