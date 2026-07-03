using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveManuscriptIdFromPageTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageTasks_Manuscripts_ManuscriptId",
                table: "PageTasks");

            migrationBuilder.DropIndex(
                name: "IX_PageTasks_ManuscriptId",
                table: "PageTasks");

            migrationBuilder.DropColumn(
                name: "ManuscriptId",
                table: "PageTasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ManuscriptId",
                table: "PageTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageTasks_ManuscriptId",
                table: "PageTasks",
                column: "ManuscriptId");

            migrationBuilder.AddForeignKey(
                name: "FK_PageTasks_Manuscripts_ManuscriptId",
                table: "PageTasks",
                column: "ManuscriptId",
                principalTable: "Manuscripts",
                principalColumn: "ManuscriptId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
