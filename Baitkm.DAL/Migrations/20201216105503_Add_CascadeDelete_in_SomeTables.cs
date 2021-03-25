using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Add_CascadeDelete_in_SomeTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Announcements_AnnouncementId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Facts_Announcements_AnnouncementId",
                table: "Facts");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_Announcements_AnnouncementId",
                table: "PersonNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscribeAnnouncements_Announcements_AnnouncementId",
                table: "SubscribeAnnouncements");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscribeAnnouncements_Guests_GuestId",
                table: "SubscribeAnnouncements");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscribeAnnouncements_Users_UserId",
                table: "SubscribeAnnouncements");

            migrationBuilder.DropForeignKey(
                name: "FK_ViewedAnnouncement_Guests_GuestId",
                table: "ViewedAnnouncement");

            migrationBuilder.DropForeignKey(
                name: "FK_ViewedAnnouncement_Users_UserId",
                table: "ViewedAnnouncement");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Announcements_AnnouncementId",
                table: "Attachments",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Facts_Announcements_AnnouncementId",
                table: "Facts",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_Announcements_AnnouncementId",
                table: "PersonNotifications",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscribeAnnouncements_Announcements_AnnouncementId",
                table: "SubscribeAnnouncements",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscribeAnnouncements_Guests_GuestId",
                table: "SubscribeAnnouncements",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscribeAnnouncements_Users_UserId",
                table: "SubscribeAnnouncements",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ViewedAnnouncement_Guests_GuestId",
                table: "ViewedAnnouncement",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ViewedAnnouncement_Users_UserId",
                table: "ViewedAnnouncement",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Announcements_AnnouncementId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Facts_Announcements_AnnouncementId",
                table: "Facts");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_Announcements_AnnouncementId",
                table: "PersonNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscribeAnnouncements_Announcements_AnnouncementId",
                table: "SubscribeAnnouncements");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscribeAnnouncements_Guests_GuestId",
                table: "SubscribeAnnouncements");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscribeAnnouncements_Users_UserId",
                table: "SubscribeAnnouncements");

            migrationBuilder.DropForeignKey(
                name: "FK_ViewedAnnouncement_Guests_GuestId",
                table: "ViewedAnnouncement");

            migrationBuilder.DropForeignKey(
                name: "FK_ViewedAnnouncement_Users_UserId",
                table: "ViewedAnnouncement");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Announcements_AnnouncementId",
                table: "Attachments",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Facts_Announcements_AnnouncementId",
                table: "Facts",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_Announcements_AnnouncementId",
                table: "PersonNotifications",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscribeAnnouncements_Announcements_AnnouncementId",
                table: "SubscribeAnnouncements",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscribeAnnouncements_Guests_GuestId",
                table: "SubscribeAnnouncements",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscribeAnnouncements_Users_UserId",
                table: "SubscribeAnnouncements",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ViewedAnnouncement_Guests_GuestId",
                table: "ViewedAnnouncement",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ViewedAnnouncement_Users_UserId",
                table: "ViewedAnnouncement",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
