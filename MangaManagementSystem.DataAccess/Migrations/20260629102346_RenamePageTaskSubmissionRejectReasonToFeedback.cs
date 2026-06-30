using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenamePageTaskSubmissionRejectReasonToFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RejectReason",
                table: "PageTaskSubmissions",
                newName: "Feedback");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Feedback",
                table: "PageTaskSubmissions",
                newName: "RejectReason");
        }
    }
}
