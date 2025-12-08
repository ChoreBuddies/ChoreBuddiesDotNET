using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class RewardModels : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DefaultRewards",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                SuggestedPoints = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DefaultRewards", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Rewards",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                HouseholdId = table.Column<int>(type: "int", nullable: false),
                Cost = table.Column<int>(type: "int", nullable: false),
                QuantityAvailable = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rewards", x => x.Id);
                table.ForeignKey(
                    name: "FK_Rewards_Households_HouseholdId",
                    column: x => x.HouseholdId,
                    principalTable: "Households",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RedeemedRewards",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UserId = table.Column<int>(type: "int", nullable: false),
                HouseholdId = table.Column<int>(type: "int", nullable: false),
                RedeemedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                PointsSpent = table.Column<int>(type: "int", nullable: false),
                IsFulfilled = table.Column<bool>(type: "bit", nullable: false),
                RewardId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RedeemedRewards", x => x.Id);
                table.ForeignKey(
                    name: "FK_RedeemedRewards_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RedeemedRewards_Households_HouseholdId",
                    column: x => x.HouseholdId,
                    principalTable: "Households",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RedeemedRewards_Rewards_RewardId",
                    column: x => x.RewardId,
                    principalTable: "Rewards",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_RedeemedRewards_HouseholdId",
            table: "RedeemedRewards",
            column: "HouseholdId");

        migrationBuilder.CreateIndex(
            name: "IX_RedeemedRewards_RewardId",
            table: "RedeemedRewards",
            column: "RewardId");

        migrationBuilder.CreateIndex(
            name: "IX_RedeemedRewards_UserId",
            table: "RedeemedRewards",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Rewards_HouseholdId",
            table: "Rewards",
            column: "HouseholdId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DefaultRewards");

        migrationBuilder.DropTable(
            name: "RedeemedRewards");

        migrationBuilder.DropTable(
            name: "Rewards");
    }
}
