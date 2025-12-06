using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class ChoreIdtoInteger : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_Chores",
            table: "Chores");

        migrationBuilder.DropColumn(
            name: "Id",
            table: "Chores");

        migrationBuilder.AddColumn<int>(
            name: "Id",
            table: "Chores",
            type: "int",
            nullable: false)
            .Annotation("SqlServer:Identity", "1, 1");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Chores",
            table: "Chores",
            column: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_Chores",
            table: "Chores");

        migrationBuilder.DropColumn(
            name: "Id",
            table: "Chores");

        migrationBuilder.AddColumn<string>(
            name: "Id",
            table: "Chores",
            type: "nvarchar(450)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Chores",
            table: "Chores",
            column: "Id");
    }
}
