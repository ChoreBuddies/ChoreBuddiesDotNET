using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChoresModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "Chores");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Chores",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chores_UserId",
                table: "Chores",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chores_AspNetUsers_UserId",
                table: "Chores",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chores_AspNetUsers_UserId",
                table: "Chores");

            migrationBuilder.DropIndex(
                name: "IX_Chores_UserId",
                table: "Chores");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Chores");

            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "Chores",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
