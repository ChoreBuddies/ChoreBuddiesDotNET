using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace ChoreBuddies.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class ChangeDefaultChoresColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DueDate",
            table: "DefaultChores");

        migrationBuilder.AddColumn<int>(
            name: "ChoreDuration",
            table: "DefaultChores",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "Frequency",
            table: "DefaultChores",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<int>(
            name: "MinAge",
            table: "DefaultChores",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ChoreDuration",
            table: "DefaultChores");

        migrationBuilder.DropColumn(
            name: "Frequency",
            table: "DefaultChores");

        migrationBuilder.DropColumn(
            name: "MinAge",
            table: "DefaultChores");

        migrationBuilder.AddColumn<DateTime>(
            name: "DueDate",
            table: "DefaultChores",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
    }
}
