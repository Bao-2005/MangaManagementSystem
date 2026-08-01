using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveChapterPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChapterPages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChapterPages",
                columns: table => new
                {
                    ChapterPageId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ChapterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageFileAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    PageNo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterPages", x => x.ChapterPageId);
                    table.ForeignKey(
                        name: "FK_ChapterPages_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChapterPages_FileAssets_ImageFileAssetId",
                        column: x => x.ImageFileAssetId,
                        principalTable: "FileAssets",
                        principalColumn: "FileAssetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPages_ChapterId_PageNo",
                table: "ChapterPages",
                columns: new[] { "ChapterId", "PageNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPages_ImageFileAssetId",
                table: "ChapterPages",
                column: "ImageFileAssetId");
        }
    }
}
