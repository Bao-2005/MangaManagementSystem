using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AvatarFileAssetId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AvatarFileAssetId",
                table: "Users",
                column: "AvatarFileAssetId",
                unique: true,
                filter: "\"AvatarFileAssetId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_FileAssets_AvatarFileAssetId",
                table: "Users",
                column: "AvatarFileAssetId",
                principalTable: "FileAssets",
                principalColumn: "FileAssetId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_FileAssets_AvatarFileAssetId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AvatarFileAssetId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarFileAssetId",
                table: "Users");
        }
    }
}
