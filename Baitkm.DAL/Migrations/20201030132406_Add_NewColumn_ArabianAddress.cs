using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Add_NewColumn_ArabianAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Announcements",
                newName: "AddressEn");

            migrationBuilder.AddColumn<string>(
                name: "AddressAr",
                table: "Announcements",
                maxLength: 300,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressAr",
                table: "Announcements");

            migrationBuilder.RenameColumn(
                name: "AddressEn",
                table: "Announcements",
                newName: "Address");
        }
    }
}
