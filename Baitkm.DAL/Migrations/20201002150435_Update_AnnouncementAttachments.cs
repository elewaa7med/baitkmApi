using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Update_AnnouncementAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnnouncementPhoto_Announcements_AnnouncementId",
                table: "AnnouncementPhoto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnnouncementPhoto",
                table: "AnnouncementPhoto");

            migrationBuilder.RenameTable(
                name: "AnnouncementPhoto",
                newName: "AnnouncementAttachments");

            migrationBuilder.RenameIndex(
                name: "IX_AnnouncementPhoto_AnnouncementId",
                table: "AnnouncementAttachments",
                newName: "IX_AnnouncementAttachments_AnnouncementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnnouncementAttachments",
                table: "AnnouncementAttachments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnnouncementAttachments_Announcements_AnnouncementId",
                table: "AnnouncementAttachments",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnnouncementAttachments_Announcements_AnnouncementId",
                table: "AnnouncementAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnnouncementAttachments",
                table: "AnnouncementAttachments");

            migrationBuilder.RenameTable(
                name: "AnnouncementAttachments",
                newName: "AnnouncementPhoto");

            migrationBuilder.RenameIndex(
                name: "IX_AnnouncementAttachments_AnnouncementId",
                table: "AnnouncementPhoto",
                newName: "IX_AnnouncementPhoto_AnnouncementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnnouncementPhoto",
                table: "AnnouncementPhoto",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnnouncementPhoto_Announcements_AnnouncementId",
                table: "AnnouncementPhoto",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
