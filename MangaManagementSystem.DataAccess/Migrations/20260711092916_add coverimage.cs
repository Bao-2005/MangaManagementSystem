using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addcoverimage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CoverImageFileAssetId",
                table: "Series",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Series_CoverImageFileAssetId",
                table: "Series",
                column: "CoverImageFileAssetId",
                unique: true,
                filter: "\"CoverImageFileAssetId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Series_FileAssets_CoverImageFileAssetId",
                table: "Series",
                column: "CoverImageFileAssetId",
                principalTable: "FileAssets",
                principalColumn: "FileAssetId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Series_FileAssets_CoverImageFileAssetId",
                table: "Series");

            migrationBuilder.DropIndex(
                name: "IX_Series_CoverImageFileAssetId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "CoverImageFileAssetId",
                table: "Series");
        }
    }
}
