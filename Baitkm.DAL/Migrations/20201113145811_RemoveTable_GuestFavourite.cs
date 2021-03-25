using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class RemoveTable_GuestFavourite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuestFavourites");

            migrationBuilder.AddColumn<int>(
                name: "AnnouncementId1",
                table: "Favourite",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestId",
                table: "Favourite",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favourite_AnnouncementId1",
                table: "Favourite",
                column: "AnnouncementId1");

            migrationBuilder.CreateIndex(
                name: "IX_Favourite_GuestId",
                table: "Favourite",
                column: "GuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favourite_Announcements_AnnouncementId1",
                table: "Favourite",
                column: "AnnouncementId1",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Favourite_Guests_GuestId",
                table: "Favourite",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favourite_Announcements_AnnouncementId1",
                table: "Favourite");

            migrationBuilder.DropForeignKey(
                name: "FK_Favourite_Guests_GuestId",
                table: "Favourite");

            migrationBuilder.DropIndex(
                name: "IX_Favourite_AnnouncementId1",
                table: "Favourite");

            migrationBuilder.DropIndex(
                name: "IX_Favourite_GuestId",
                table: "Favourite");

            migrationBuilder.DropColumn(
                name: "AnnouncementId1",
                table: "Favourite");

            migrationBuilder.DropColumn(
                name: "GuestId",
                table: "Favourite");

            migrationBuilder.CreateTable(
                name: "GuestFavourites",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AnnouncementId = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    GuestId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestFavourites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestFavourites_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuestFavourites_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuestFavourites_AnnouncementId",
                table: "GuestFavourites",
                column: "AnnouncementId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestFavourites_GuestId",
                table: "GuestFavourites",
                column: "GuestId");
        }
    }
}
