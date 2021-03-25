using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class UserId_NotNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_Users_UserId",
                table: "PersonNotifications");

            migrationBuilder.DropIndex(
                name: "IX_PersonNotifications_UserId",
                table: "PersonNotifications");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "PersonNotifications",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "PersonNotifications",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonNotifications_UserId1",
                table: "PersonNotifications",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_Users_UserId1",
                table: "PersonNotifications",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_Users_UserId1",
                table: "PersonNotifications");

            migrationBuilder.DropIndex(
                name: "IX_PersonNotifications_UserId1",
                table: "PersonNotifications");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PersonNotifications");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "PersonNotifications",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateIndex(
                name: "IX_PersonNotifications_UserId",
                table: "PersonNotifications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_Users_UserId",
                table: "PersonNotifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
