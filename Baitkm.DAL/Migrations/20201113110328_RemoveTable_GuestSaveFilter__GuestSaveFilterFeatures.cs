using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class RemoveTable_GuestSaveFilter__GuestSaveFilterFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilterFeatures_SaveFilters_SaveFilterId",
                table: "SaveFilterFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilters_Cities_CityId",
                table: "SaveFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilters_Users_UserId",
                table: "SaveFilters");

            migrationBuilder.DropTable(
                name: "GuestSaveFilterAnnouncementFeatures");

            migrationBuilder.DropTable(
                name: "GuestSaveFilters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveFilters",
                table: "SaveFilters");

            migrationBuilder.RenameTable(
                name: "SaveFilters",
                newName: "SaveFilter");

            migrationBuilder.RenameIndex(
                name: "IX_SaveFilters_UserId",
                table: "SaveFilter",
                newName: "IX_SaveFilter_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SaveFilters_CityId",
                table: "SaveFilter",
                newName: "IX_SaveFilter_CityId");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "SaveFilter",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SaveFilter",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestId",
                table: "SaveFilter",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveFilter",
                table: "SaveFilter",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SaveFilter_GuestId",
                table: "SaveFilter",
                column: "GuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaveFilter_Cities_CityId",
                table: "SaveFilter",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SaveFilter_Guests_GuestId",
                table: "SaveFilter",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SaveFilter_Users_UserId",
                table: "SaveFilter",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SaveFilterFeatures_SaveFilter_SaveFilterId",
                table: "SaveFilterFeatures",
                column: "SaveFilterId",
                principalTable: "SaveFilter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilter_Cities_CityId",
                table: "SaveFilter");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilter_Guests_GuestId",
                table: "SaveFilter");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilter_Users_UserId",
                table: "SaveFilter");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilterFeatures_SaveFilter_SaveFilterId",
                table: "SaveFilterFeatures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveFilter",
                table: "SaveFilter");

            migrationBuilder.DropIndex(
                name: "IX_SaveFilter_GuestId",
                table: "SaveFilter");

            migrationBuilder.DropColumn(
                name: "GuestId",
                table: "SaveFilter");

            migrationBuilder.RenameTable(
                name: "SaveFilter",
                newName: "SaveFilters");

            migrationBuilder.RenameIndex(
                name: "IX_SaveFilter_UserId",
                table: "SaveFilters",
                newName: "IX_SaveFilters_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SaveFilter_CityId",
                table: "SaveFilters",
                newName: "IX_SaveFilters_CityId");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "SaveFilters",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SaveFilters",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveFilters",
                table: "SaveFilters",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GuestSaveFilters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Address = table.Column<string>(maxLength: 100, nullable: true),
                    AnnouncementEstateType = table.Column<int>(nullable: true),
                    AnnouncementRentType = table.Column<int>(nullable: true),
                    AnnouncementResidentialType = table.Column<int>(nullable: true),
                    AnnouncementType = table.Column<int>(nullable: true),
                    BathroomCount = table.Column<int>(nullable: true),
                    BedroomCount = table.Column<int>(nullable: true),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 100, nullable: true),
                    FilterCount = table.Column<int>(nullable: false),
                    GuestId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    Lat = table.Column<decimal>(type: "decimal(18, 6)", nullable: false),
                    Lng = table.Column<decimal>(type: "decimal(18, 6)", nullable: false),
                    MaxArea = table.Column<decimal>(nullable: true),
                    MinArea = table.Column<decimal>(nullable: true),
                    PriceFrom = table.Column<decimal>(nullable: true),
                    PriceTo = table.Column<decimal>(nullable: true),
                    SaveFilterName = table.Column<string>(maxLength: 100, nullable: true),
                    Search = table.Column<string>(maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestSaveFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestSaveFilters_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuestSaveFilterAnnouncementFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    FeatureType = table.Column<int>(nullable: false),
                    GuestSaveFilterId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestSaveFilterAnnouncementFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestSaveFilterAnnouncementFeatures_GuestSaveFilters_GuestSaveFilterId",
                        column: x => x.GuestSaveFilterId,
                        principalTable: "GuestSaveFilters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuestSaveFilterAnnouncementFeatures_GuestSaveFilterId",
                table: "GuestSaveFilterAnnouncementFeatures",
                column: "GuestSaveFilterId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestSaveFilters_GuestId",
                table: "GuestSaveFilters",
                column: "GuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaveFilterFeatures_SaveFilters_SaveFilterId",
                table: "SaveFilterFeatures",
                column: "SaveFilterId",
                principalTable: "SaveFilters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaveFilters_Cities_CityId",
                table: "SaveFilters",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SaveFilters_Users_UserId",
                table: "SaveFilters",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
