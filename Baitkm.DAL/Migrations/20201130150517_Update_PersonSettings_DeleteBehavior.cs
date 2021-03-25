using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Update_PersonSettings_DeleteBehavior : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonOtherSettings_Guests_GuestId",
                table: "PersonOtherSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonOtherSettings_Users_UserId",
                table: "PersonOtherSettings");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonOtherSettings_Guests_GuestId",
                table: "PersonOtherSettings",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonOtherSettings_Users_UserId",
                table: "PersonOtherSettings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonOtherSettings_Guests_GuestId",
                table: "PersonOtherSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonOtherSettings_Users_UserId",
                table: "PersonOtherSettings");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonOtherSettings_Guests_GuestId",
                table: "PersonOtherSettings",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonOtherSettings_Users_UserId",
                table: "PersonOtherSettings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
