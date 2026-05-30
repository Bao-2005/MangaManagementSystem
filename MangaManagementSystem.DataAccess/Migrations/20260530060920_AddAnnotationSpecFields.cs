using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnotationSpecFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Annotations_ChapterPages_ChapterPageId",
                table: "Annotations");

            migrationBuilder.DropIndex(
                name: "IX_Annotations_ManuscriptId",
                table: "Annotations");

            migrationBuilder.AlterColumn<decimal>(
                name: "PositionY",
                table: "Annotations",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "PositionX",
                table: "Annotations",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Annotations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterPageId",
                table: "Annotations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Annotations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Annotations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersionNo",
                table: "Annotations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_ManuscriptId_VersionNo",
                table: "Annotations",
                columns: new[] { "ManuscriptId", "VersionNo" });

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_ManuscriptId_VersionNo_PageNo",
                table: "Annotations",
                columns: new[] { "ManuscriptId", "VersionNo", "PageNo" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Annotation_PageNo",
                table: "Annotations",
                sql: "[PageNo] >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Annotation_PositionX",
                table: "Annotations",
                sql: "[PositionX] >= 0 AND [PositionX] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Annotation_PositionY",
                table: "Annotations",
                sql: "[PositionY] >= 0 AND [PositionY] <= 100");

            migrationBuilder.AddForeignKey(
                name: "FK_Annotations_ChapterPages_ChapterPageId",
                table: "Annotations",
                column: "ChapterPageId",
                principalTable: "ChapterPages",
                principalColumn: "ChapterPageId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Annotations_ChapterPages_ChapterPageId",
                table: "Annotations");

            migrationBuilder.DropIndex(
                name: "IX_Annotations_ManuscriptId_VersionNo",
                table: "Annotations");

            migrationBuilder.DropIndex(
                name: "IX_Annotations_ManuscriptId_VersionNo_PageNo",
                table: "Annotations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Annotation_PageNo",
                table: "Annotations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Annotation_PositionX",
                table: "Annotations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Annotation_PositionY",
                table: "Annotations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Annotations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Annotations");

            migrationBuilder.DropColumn(
                name: "VersionNo",
                table: "Annotations");

            migrationBuilder.AlterColumn<decimal>(
                name: "PositionY",
                table: "Annotations",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PositionX",
                table: "Annotations",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Annotations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<Guid>(
                name: "ChapterPageId",
                table: "Annotations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_ManuscriptId",
                table: "Annotations",
                column: "ManuscriptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Annotations_ChapterPages_ChapterPageId",
                table: "Annotations",
                column: "ChapterPageId",
                principalTable: "ChapterPages",
                principalColumn: "ChapterPageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
