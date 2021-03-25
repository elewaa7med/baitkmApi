using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class PersonNotification_Table_AnnouncementId_NotRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AnnouncementId",
                table: "PersonNotifications",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AnnouncementId",
                table: "PersonNotifications",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
