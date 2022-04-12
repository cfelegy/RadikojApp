using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaspApp.Migrations
{
    public partial class addaccountfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Disabled",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastLoggedInAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SuperUser",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Disabled",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LastLoggedInAt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SuperUser",
                table: "Accounts");
        }
    }
}
