using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Update_SubscribeAnnouncements_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestSubscribeAnnouncements_Announcements_AnnouncementId",
                table: "GuestSubscribeAnnouncements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscribeAnnouncements_Announcements_AnnouncementId",
                table: "UserSubscribeAnnouncements");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserSubscribeAnnouncements",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "UserSubscribeAnnouncements",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "GuestSubscribeAnnouncements",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "GuestSubscribeAnnouncements",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GuestSubscribeAnnouncements_Announcements_AnnouncementId",
                table: "GuestSubscribeAnnouncements",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscribeAnnouncements_Announcements_AnnouncementId",
                table: "UserSubscribeAnnouncements",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestSubscribeAnnouncements_Announcements_AnnouncementId",
                table: "GuestSubscribeAnnouncements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscribeAnnouncements_Announcements_AnnouncementId",
                table: "UserSubscribeAnnouncements");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserSubscribeAnnouncements",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "UserSubscribeAnnouncements",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "GuestSubscribeAnnouncements",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "GuestSubscribeAnnouncements",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_GuestSubscribeAnnouncements_Announcements_AnnouncementId",
                table: "GuestSubscribeAnnouncements",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscribeAnnouncements_Announcements_AnnouncementId",
                table: "UserSubscribeAnnouncements",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
