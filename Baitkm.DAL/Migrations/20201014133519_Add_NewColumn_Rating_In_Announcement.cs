using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Add_NewColumn_Rating_In_Announcement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Announcements",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Announcements");
        }
    }
}
