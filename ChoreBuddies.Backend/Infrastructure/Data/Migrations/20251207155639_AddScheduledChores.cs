using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddScheduledChores : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ScheduledChores",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                MinAge = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Room = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RewardPointsCount = table.Column<int>(type: "int", nullable: false),
                HouseholdId = table.Column<int>(type: "int", nullable: false),
                ChoreDuration = table.Column<int>(type: "int", nullable: false),
                Frequency = table.Column<int>(type: "int", nullable: false),
                LastGenerated = table.Column<DateTime>(type: "datetime2", nullable: true),
                EveryX = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ScheduledChores", x => x.Id);
                table.ForeignKey(
                    name: "FK_ScheduledChores_Households_HouseholdId",
                    column: x => x.HouseholdId,
                    principalTable: "Households",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ScheduledChores_HouseholdId",
            table: "ScheduledChores",
            column: "HouseholdId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ScheduledChores");
    }
}
