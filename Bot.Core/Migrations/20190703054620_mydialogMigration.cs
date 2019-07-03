using Microsoft.EntityFrameworkCore.Migrations;

namespace StuddyBot.Core.Migrations
{
    public partial class mydialogMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dialog_Dialogs_DialogsId",
                table: "Dialog");

            migrationBuilder.DropColumn(
                name: "DialogId",
                table: "Dialog");

            migrationBuilder.AlterColumn<int>(
                name: "DialogsId",
                table: "Dialog",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialog_Dialogs_DialogsId",
                table: "Dialog",
                column: "DialogsId",
                principalTable: "Dialogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dialog_Dialogs_DialogsId",
                table: "Dialog");

            migrationBuilder.AlterColumn<int>(
                name: "DialogsId",
                table: "Dialog",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "DialogId",
                table: "Dialog",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialog_Dialogs_DialogsId",
                table: "Dialog",
                column: "DialogsId",
                principalTable: "Dialogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
