using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Add_PushNotificationTranslate_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PushNotifications",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PushNotificationId",
                table: "PersonNotifications",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PushNotificationTranslate",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PushNotificationId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Language = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushNotificationTranslate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushNotificationTranslate_PushNotifications_PushNotificationId",
                        column: x => x.PushNotificationId,
                        principalTable: "PushNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_UserId",
                table: "PushNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonNotifications_PushNotificationId",
                table: "PersonNotifications",
                column: "PushNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotificationTranslate_PushNotificationId",
                table: "PushNotificationTranslate",
                column: "PushNotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_PushNotifications_PushNotificationId",
                table: "PersonNotifications",
                column: "PushNotificationId",
                principalTable: "PushNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PushNotifications_Users_UserId",
                table: "PushNotifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_PushNotifications_PushNotificationId",
                table: "PersonNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PushNotifications_Users_UserId",
                table: "PushNotifications");

            migrationBuilder.DropTable(
                name: "PushNotificationTranslate");

            migrationBuilder.DropIndex(
                name: "IX_PushNotifications_UserId",
                table: "PushNotifications");

            migrationBuilder.DropIndex(
                name: "IX_PersonNotifications_PushNotificationId",
                table: "PersonNotifications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PushNotifications");

            migrationBuilder.DropColumn(
                name: "PushNotificationId",
                table: "PersonNotifications");
        }
    }
}
