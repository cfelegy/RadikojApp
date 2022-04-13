using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Radikoj.Migrations
{
    public partial class addsurveyresponder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ResponderId",
                table: "SurveyResponses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponderId",
                table: "SurveyResponses");
        }
    }
}
