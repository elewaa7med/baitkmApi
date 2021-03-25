using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Remove_Facts_Announcements_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscribeAnnouncements_Announcements_AnnouncementId",
                table: "UserSubscribeAnnouncements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscribeAnnouncements_Users_UserId",
                table: "UserSubscribeAnnouncements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSubscribeAnnouncements",
                table: "UserSubscribeAnnouncements");

            migrationBuilder.RenameTable(
                name: "UserSubscribeAnnouncements",
                newName: "UserSubscribeAnnouncement");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscribeAnnouncements_UserId",
                table: "UserSubscribeAnnouncement",
                newName: "IX_UserSubscribeAnnouncement_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscribeAnnouncements_AnnouncementId",
                table: "UserSubscribeAnnouncement",
                newName: "IX_UserSubscribeAnnouncement_AnnouncementId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "UserSubscribeAnnouncement",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "UserSubscribeAnnouncement",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSubscribeAnnouncement",
                table: "UserSubscribeAnnouncement",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GuestSubscribeAnnouncement",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    AnnouncementId = table.Column<int>(nullable: false),
                    GuestId = table.Column<int>(nullable: false),
                    Email = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestSubscribeAnnouncement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestSubscribeAnnouncement_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuestSubscribeAnnouncement_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuestSubscribeAnnouncement_AnnouncementId",
                table: "GuestSubscribeAnnouncement",
                column: "AnnouncementId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestSubscribeAnnouncement_GuestId",
                table: "GuestSubscribeAnnouncement",
                column: "GuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscribeAnnouncement_Announcements_AnnouncementId",
                table: "UserSubscribeAnnouncement",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscribeAnnouncement_Users_UserId",
                table: "UserSubscribeAnnouncement",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscribeAnnouncement_Announcements_AnnouncementId",
                table: "UserSubscribeAnnouncement");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscribeAnnouncement_Users_UserId",
                table: "UserSubscribeAnnouncement");

            migrationBuilder.DropTable(
                name: "GuestSubscribeAnnouncement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSubscribeAnnouncement",
                table: "UserSubscribeAnnouncement");

            migrationBuilder.RenameTable(
                name: "UserSubscribeAnnouncement",
                newName: "UserSubscribeAnnouncements");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscribeAnnouncement_UserId",
                table: "UserSubscribeAnnouncements",
                newName: "IX_UserSubscribeAnnouncements_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscribeAnnouncement_AnnouncementId",
                table: "UserSubscribeAnnouncements",
                newName: "IX_UserSubscribeAnnouncements_AnnouncementId");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSubscribeAnnouncements",
                table: "UserSubscribeAnnouncements",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscribeAnnouncements_Announcements_AnnouncementId",
                table: "UserSubscribeAnnouncements",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscribeAnnouncements_Users_UserId",
                table: "UserSubscribeAnnouncements",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
