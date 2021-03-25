using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Add_Configuration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_Announcements_AnnouncementId",
                table: "PersonNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_PushNotifications_PushNotificationId",
                table: "PersonNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_Users_UserId",
                table: "PersonNotifications");

            migrationBuilder.AlterColumn<int>(
                name: "NotificationId",
                table: "PersonNotifications",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_Announcements_AnnouncementId",
                table: "PersonNotifications",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_PushNotifications_PushNotificationId",
                table: "PersonNotifications",
                column: "PushNotificationId",
                principalTable: "PushNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_Users_UserId",
                table: "PersonNotifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_Announcements_AnnouncementId",
                table: "PersonNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_PushNotifications_PushNotificationId",
                table: "PersonNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_Users_UserId",
                table: "PersonNotifications");

            migrationBuilder.AlterColumn<int>(
                name: "NotificationId",
                table: "PersonNotifications",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_Announcements_AnnouncementId",
                table: "PersonNotifications",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_PushNotifications_PushNotificationId",
                table: "PersonNotifications",
                column: "PushNotificationId",
                principalTable: "PushNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_Users_UserId",
                table: "PersonNotifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
