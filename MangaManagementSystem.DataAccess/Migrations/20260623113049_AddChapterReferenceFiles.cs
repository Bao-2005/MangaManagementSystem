using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddChapterReferenceFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChapterReferenceFiles",
                columns: table => new
                {
                    ChapterReferenceFileId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ChapterId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterReferenceFiles", x => x.ChapterReferenceFileId);
                    table.ForeignKey(
                        name: "FK_ChapterReferenceFiles_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChapterReferenceFiles_FileAssets_FileAssetId",
                        column: x => x.FileAssetId,
                        principalTable: "FileAssets",
                        principalColumn: "FileAssetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChapterReferenceFiles_ChapterId_FileAssetId",
                table: "ChapterReferenceFiles",
                columns: new[] { "ChapterId", "FileAssetId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterReferenceFiles_FileAssetId",
                table: "ChapterReferenceFiles",
                column: "FileAssetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChapterReferenceFiles");
        }
    }
}
