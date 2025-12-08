using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddInvitationCodeToHousehold : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "InvitationCode",
            table: "Households",
            type: "nvarchar(6)",
            maxLength: 6,
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "InvitationCode",
            table: "Households");
    }
}
