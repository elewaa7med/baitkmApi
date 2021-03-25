using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Baitkm.DAL.Migrations
{
    public partial class Delete_Guest_and_User_Settings_Merged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilter_Guests_GuestId",
                table: "SaveFilter");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilter_Users_UserId",
                table: "SaveFilter");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilterFeatures_SaveFilter_SaveFilterId",
                table: "SaveFilterFeatures");

            migrationBuilder.DropTable(
                name: "GuestSubscriptionAreaAndLanguage");

            migrationBuilder.DropTable(
                name: "GuestSubscriptions");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "UserSubscriptionAreaAndLanguages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveFilter",
                table: "SaveFilter");

            migrationBuilder.RenameTable(
                name: "SaveFilter",
                newName: "SaveFilters");

            migrationBuilder.RenameIndex(
                name: "IX_SaveFilter_UserId",
                table: "SaveFilters",
                newName: "IX_SaveFilters_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SaveFilter_GuestId",
                table: "SaveFilters",
                newName: "IX_SaveFilters_GuestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveFilters",
                table: "SaveFilters",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PersonOtherSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    AreaUnit = table.Column<int>(nullable: false),
                    Language = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    GuestId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonOtherSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonOtherSettings_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonOtherSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    SubscriptionsType = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    GuestId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonSettings_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonOtherSettings_GuestId",
                table: "PersonOtherSettings",
                column: "GuestId",
                unique: true,
                filter: "[GuestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonOtherSettings_UserId",
                table: "PersonOtherSettings",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonSettings_GuestId",
                table: "PersonSettings",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonSettings_UserId",
                table: "PersonSettings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaveFilterFeatures_SaveFilters_SaveFilterId",
                table: "SaveFilterFeatures",
                column: "SaveFilterId",
                principalTable: "SaveFilters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaveFilters_Guests_GuestId",
                table: "SaveFilters",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SaveFilters_Users_UserId",
                table: "SaveFilters",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilterFeatures_SaveFilters_SaveFilterId",
                table: "SaveFilterFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilters_Guests_GuestId",
                table: "SaveFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_SaveFilters_Users_UserId",
                table: "SaveFilters");

            migrationBuilder.DropTable(
                name: "PersonOtherSettings");

            migrationBuilder.DropTable(
                name: "PersonSettings");

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
                name: "IX_SaveFilters_GuestId",
                table: "SaveFilter",
                newName: "IX_SaveFilter_GuestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveFilter",
                table: "SaveFilter",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GuestSubscriptionAreaAndLanguage",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AreaUnit = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    GuestId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    Language = table.Column<int>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestSubscriptionAreaAndLanguage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestSubscriptionAreaAndLanguage_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuestSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    GuestId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    SubscriptionsType = table.Column<int>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestSubscriptions_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    SubscriptionsType = table.Column<int>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptionAreaAndLanguages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AreaUnit = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDt = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                    Language = table.Column<int>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    UpdatedDt = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptionAreaAndLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptionAreaAndLanguages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuestSubscriptionAreaAndLanguage_GuestId",
                table: "GuestSubscriptionAreaAndLanguage",
                column: "GuestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestSubscriptions_GuestId",
                table: "GuestSubscriptions",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionAreaAndLanguages_UserId",
                table: "UserSubscriptionAreaAndLanguages",
                column: "UserId",
                unique: true);

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
    }
}
