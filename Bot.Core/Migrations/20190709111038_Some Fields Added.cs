using Microsoft.EntityFrameworkCore.Migrations;

namespace StuddyBot.Core.Migrations
{
    public partial class SomeFieldsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Courses",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Courses");
        }
    }
}
