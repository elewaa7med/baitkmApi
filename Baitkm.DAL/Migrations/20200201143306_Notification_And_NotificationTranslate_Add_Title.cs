using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Notification_And_NotificationTranslate_Add_Title : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "NotificationTranslate",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Notification",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "NotificationTranslate");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Notification");
        }
    }
}
