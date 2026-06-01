using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddManuscriptSubmitterFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedBy",
                table: "Manuscripts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevisionCount",
                table: "Manuscripts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedBy",
                table: "Manuscripts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Manuscripts_ReviewedBy",
                table: "Manuscripts",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Manuscripts_SubmittedBy",
                table: "Manuscripts",
                column: "SubmittedBy");

            // Trước khi tạo FK constraint, cần đảm bảo mọi row có SubmittedBy trỏ tới UserId hợp lệ.
            // Các row cũ (trước khi có field này) sẽ có SubmittedBy = '00000000-...' (default Guid).
            // SQL này cập nhật chúng bằng UserId đầu tiên tìm thấy trong bảng Users.
            // Nếu bảng Manuscripts rỗng, câu lệnh này không làm gì.
            migrationBuilder.Sql(@"
                DECLARE @defaultUserId UNIQUEIDENTIFIER = (SELECT TOP 1 UserId FROM [Users] ORDER BY CreatedAt);
                IF @defaultUserId IS NOT NULL
                BEGIN
                    UPDATE [Manuscripts]
                    SET [SubmittedBy] = @defaultUserId
                    WHERE [SubmittedBy] = '00000000-0000-0000-0000-000000000000';
                END
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_Manuscripts_Users_ReviewedBy",
                table: "Manuscripts",
                column: "ReviewedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Manuscripts_Users_SubmittedBy",
                table: "Manuscripts",
                column: "SubmittedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Manuscripts_Users_ReviewedBy",
                table: "Manuscripts");

            migrationBuilder.DropForeignKey(
                name: "FK_Manuscripts_Users_SubmittedBy",
                table: "Manuscripts");

            migrationBuilder.DropIndex(
                name: "IX_Manuscripts_ReviewedBy",
                table: "Manuscripts");

            migrationBuilder.DropIndex(
                name: "IX_Manuscripts_SubmittedBy",
                table: "Manuscripts");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "Manuscripts");

            migrationBuilder.DropColumn(
                name: "RevisionCount",
                table: "Manuscripts");

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                table: "Manuscripts");
        }
    }
}
