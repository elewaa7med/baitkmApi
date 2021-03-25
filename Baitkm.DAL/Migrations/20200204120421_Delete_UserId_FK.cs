using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Delete_UserId_FK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PushNotifications_Users_UserId",
                table: "PushNotifications");

            migrationBuilder.DropIndex(
                name: "IX_PushNotifications_UserId",
                table: "PushNotifications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PushNotifications");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PushNotifications",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_UserId",
                table: "PushNotifications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PushNotifications_Users_UserId",
                table: "PushNotifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
