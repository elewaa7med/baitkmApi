using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Notification_And_NotificationTranslate_Tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnouncementRejects");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PersonNotifications");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "PersonNotifications");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "PersonNotifications");

            migrationBuilder.AddColumn<int>(
                name: "NotificationId",
                table: "PersonNotifications",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTranslate",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NotificationId = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Language = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTranslate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTranslate_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonNotifications_NotificationId",
                table: "PersonNotifications",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTranslate_NotificationId",
                table: "NotificationTranslate",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_Notification_NotificationId",
                table: "PersonNotifications",
                column: "NotificationId",
                principalTable: "Notification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_Notification_NotificationId",
                table: "PersonNotifications");

            migrationBuilder.DropTable(
                name: "NotificationTranslate");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_PersonNotifications_NotificationId",
                table: "PersonNotifications");

            migrationBuilder.DropColumn(
                name: "NotificationId",
                table: "PersonNotifications");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PersonNotifications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "PersonNotifications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "PersonNotifications",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnnouncementRejects",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AnnouncementId = table.Column<int>(nullable: false),
                    AnnouncementRejectStatus = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementRejects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnnouncementRejects_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementRejects_AnnouncementId",
                table: "AnnouncementRejects",
                column: "AnnouncementId");
        }
    }
}
