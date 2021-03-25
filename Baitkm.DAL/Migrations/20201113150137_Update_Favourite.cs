using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Update_Favourite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favourite_Announcements_AnnouncementId1",
                table: "Favourite");

            migrationBuilder.DropIndex(
                name: "IX_Favourite_AnnouncementId1",
                table: "Favourite");

            migrationBuilder.DropColumn(
                name: "AnnouncementId1",
                table: "Favourite");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnnouncementId1",
                table: "Favourite",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favourite_AnnouncementId1",
                table: "Favourite",
                column: "AnnouncementId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Favourite_Announcements_AnnouncementId1",
                table: "Favourite",
                column: "AnnouncementId1",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
