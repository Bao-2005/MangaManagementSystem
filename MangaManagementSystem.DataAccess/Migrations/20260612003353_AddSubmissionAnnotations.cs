using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionAnnotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PageTaskSubmissionId",
                table: "Annotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_PageTaskSubmissionId",
                table: "Annotations",
                column: "PageTaskSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Annotations_PageTaskSubmissions_PageTaskSubmissionId",
                table: "Annotations",
                column: "PageTaskSubmissionId",
                principalTable: "PageTaskSubmissions",
                principalColumn: "SubmissionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Annotations_PageTaskSubmissions_PageTaskSubmissionId",
                table: "Annotations");

            migrationBuilder.DropIndex(
                name: "IX_Annotations_PageTaskSubmissionId",
                table: "Annotations");

            migrationBuilder.DropColumn(
                name: "PageTaskSubmissionId",
                table: "Annotations");
        }
    }
}
