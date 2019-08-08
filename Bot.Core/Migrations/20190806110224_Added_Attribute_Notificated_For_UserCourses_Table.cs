using Microsoft.EntityFrameworkCore.Migrations;

namespace StuddyBot.Core.Migrations
{
    public partial class Added_Attribute_Notificated_For_UserCourses_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Notified",
                table: "UserCourses",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notified",
                table: "UserCourses");
        }
    }
}
