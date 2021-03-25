using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Add_GuestFk_in_PersonNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "PersonNotifications",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "GuestId",
                table: "PersonNotifications",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonNotifications_GuestId",
                table: "PersonNotifications",
                column: "GuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonNotifications_Guests_GuestId",
                table: "PersonNotifications",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonNotifications_Guests_GuestId",
                table: "PersonNotifications");

            migrationBuilder.DropIndex(
                name: "IX_PersonNotifications_GuestId",
                table: "PersonNotifications");

            migrationBuilder.DropColumn(
                name: "GuestId",
                table: "PersonNotifications");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "PersonNotifications",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
