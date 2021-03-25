using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLocation_Users_UserId",
                table: "UserLocation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLocation",
                table: "UserLocation");

            migrationBuilder.RenameTable(
                name: "UserLocation",
                newName: "UserLocations");

            migrationBuilder.RenameIndex(
                name: "IX_UserLocation_UserId",
                table: "UserLocations",
                newName: "IX_UserLocations_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLocations",
                table: "UserLocations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLocations_Users_UserId",
                table: "UserLocations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLocations_Users_UserId",
                table: "UserLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLocations",
                table: "UserLocations");

            migrationBuilder.RenameTable(
                name: "UserLocations",
                newName: "UserLocation");

            migrationBuilder.RenameIndex(
                name: "IX_UserLocations_UserId",
                table: "UserLocation",
                newName: "IX_UserLocation_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLocation",
                table: "UserLocation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLocation_Users_UserId",
                table: "UserLocation",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
