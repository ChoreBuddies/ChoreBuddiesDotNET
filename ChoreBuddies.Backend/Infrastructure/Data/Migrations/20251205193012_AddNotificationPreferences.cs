using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddNotificationPreferences : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "NotificationPreference",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Channel = table.Column<int>(type: "int", nullable: false),
                IsEnabled = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NotificationPreference", x => x.Id);
                table.ForeignKey(
                    name: "FK_NotificationPreference_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_NotificationPreference_UserId",
            table: "NotificationPreference",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "NotificationPreference");
    }
}
