using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaspApp.Migrations
{
    public partial class newauth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Accounts",
                newName: "LoginToken");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LoginTokenExpiresAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoginTokenExpiresAt",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "LoginToken",
                table: "Accounts",
                newName: "PasswordHash");
        }
    }
}
