using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaspApp.Migrations
{
    public partial class attachsurveyitems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyItem_Surveys_SurveyId",
                table: "SurveyItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SurveyItem",
                table: "SurveyItem");

            migrationBuilder.RenameTable(
                name: "SurveyItem",
                newName: "SurveyItems");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyItem_SurveyId",
                table: "SurveyItems",
                newName: "IX_SurveyItems_SurveyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurveyItems",
                table: "SurveyItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyItems_Surveys_SurveyId",
                table: "SurveyItems",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyItems_Surveys_SurveyId",
                table: "SurveyItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SurveyItems",
                table: "SurveyItems");

            migrationBuilder.RenameTable(
                name: "SurveyItems",
                newName: "SurveyItem");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyItems_SurveyId",
                table: "SurveyItem",
                newName: "IX_SurveyItem_SurveyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurveyItem",
                table: "SurveyItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyItem_Surveys_SurveyId",
                table: "SurveyItem",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id");
        }
    }
}
