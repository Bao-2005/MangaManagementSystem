using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPageTaskReferenceFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PageTaskReferenceFiles",
                columns: table => new
                {
                    PageTaskReferenceFileId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PageTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageTaskReferenceFiles", x => x.PageTaskReferenceFileId);
                    table.ForeignKey(
                        name: "FK_PageTaskReferenceFiles_FileAssets_FileAssetId",
                        column: x => x.FileAssetId,
                        principalTable: "FileAssets",
                        principalColumn: "FileAssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageTaskReferenceFiles_PageTasks_PageTaskId",
                        column: x => x.PageTaskId,
                        principalTable: "PageTasks",
                        principalColumn: "PageTaskId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageTaskReferenceFiles_FileAssetId",
                table: "PageTaskReferenceFiles",
                column: "FileAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_PageTaskReferenceFiles_PageTaskId_FileAssetId",
                table: "PageTaskReferenceFiles",
                columns: new[] { "PageTaskId", "FileAssetId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageTaskReferenceFiles");
        }
    }
}
