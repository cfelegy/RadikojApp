using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Radikoj.Migrations
{
    public partial class addtranlsationgroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "LocalizedItems",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Group",
                table: "LocalizedItems");
        }
    }
}
