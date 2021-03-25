using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Update_SubAnnouncement_Table_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnouncementStatus",
                table: "UserSubscribeAnnouncements");

            migrationBuilder.DropColumn(
                name: "AnnouncementStatus",
                table: "GuestSubscribeAnnouncements");

            migrationBuilder.AddColumn<int>(
                name: "AnnouncementType",
                table: "UserSubscribeAnnouncements",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AnnouncementType",
                table: "GuestSubscribeAnnouncements",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnouncementType",
                table: "UserSubscribeAnnouncements");

            migrationBuilder.DropColumn(
                name: "AnnouncementType",
                table: "GuestSubscribeAnnouncements");

            migrationBuilder.AddColumn<int>(
                name: "AnnouncementStatus",
                table: "UserSubscribeAnnouncements",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AnnouncementStatus",
                table: "GuestSubscribeAnnouncements",
                nullable: false,
                defaultValue: 0);
        }
    }
}
