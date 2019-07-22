using Microsoft.EntityFrameworkCore.Migrations;

namespace StuddyBot.Core.Migrations
{
    public partial class UpdatedFieldInUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConversationReference",
                table: "User",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversationReference",
                table: "User");
        }
    }
}
