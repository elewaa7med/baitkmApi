using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Update_SubAnnouncement_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "UserSubscribeAnnouncements",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnnouncementEstateType",
                table: "UserSubscribeAnnouncements",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AnnouncementStatus",
                table: "UserSubscribeAnnouncements",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "GuestSubscribeAnnouncements",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnnouncementEstateType",
                table: "GuestSubscribeAnnouncements",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AnnouncementStatus",
                table: "GuestSubscribeAnnouncements",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "UserSubscribeAnnouncements");

            migrationBuilder.DropColumn(
                name: "AnnouncementEstateType",
                table: "UserSubscribeAnnouncements");

            migrationBuilder.DropColumn(
                name: "AnnouncementStatus",
                table: "UserSubscribeAnnouncements");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "GuestSubscribeAnnouncements");

            migrationBuilder.DropColumn(
                name: "AnnouncementEstateType",
                table: "GuestSubscribeAnnouncements");

            migrationBuilder.DropColumn(
                name: "AnnouncementStatus",
                table: "GuestSubscribeAnnouncements");
        }
    }
}
