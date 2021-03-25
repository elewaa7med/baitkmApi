using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Add_NewColumn_CityName_CountryName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilter_Cities_CityId",
                table: "SaveFilter");

            migrationBuilder.DropIndex(
                name: "IX_SaveFilter_CityId",
                table: "SaveFilter");

            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "SaveFilter",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryName",
                table: "SaveFilter",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityName",
                table: "SaveFilter");

            migrationBuilder.DropColumn(
                name: "CountryName",
                table: "SaveFilter");

            migrationBuilder.CreateIndex(
                name: "IX_SaveFilter_CityId",
                table: "SaveFilter",
                column: "CityId",
                unique: true,
                filter: "[CityId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SaveFilter_Cities_CityId",
                table: "SaveFilter",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
