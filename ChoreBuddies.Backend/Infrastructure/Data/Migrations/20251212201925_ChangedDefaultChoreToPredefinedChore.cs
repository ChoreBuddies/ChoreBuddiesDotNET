using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class ChangedDefaultChoreToPredefinedChore : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DefaultChores");

        migrationBuilder.CreateTable(
            name: "PredefinedChores",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Room = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RewardPointsCount = table.Column<int>(type: "int", nullable: false),
                ChoreDuration = table.Column<int>(type: "int", nullable: false),
                Frequency = table.Column<int>(type: "int", nullable: false),
                EveryX = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PredefinedChores", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PredefinedChores");

        migrationBuilder.CreateTable(
            name: "DefaultChores",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ChoreDuration = table.Column<int>(type: "int", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                MinAge = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RewardPointsCount = table.Column<int>(type: "int", nullable: false),
                Room = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DefaultChores", x => x.Id);
            });
    }
}
