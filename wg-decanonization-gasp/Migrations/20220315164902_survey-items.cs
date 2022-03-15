using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaspApp.Migrations
{
    public partial class surveyitems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemContents",
                table: "SurveyItem",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ItemType",
                table: "SurveyItem",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemContents",
                table: "SurveyItem");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "SurveyItem");
        }
    }
}
