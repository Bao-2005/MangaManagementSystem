using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnotationOwnerCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Annotations\" SET \"ManuscriptId\" = NULL WHERE \"PageTaskSubmissionId\" IS NOT NULL;");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Annotations_ExactlyOneOwner",
                table: "Annotations",
                sql: "(\"ManuscriptId\" IS NOT NULL AND \"PageTaskSubmissionId\" IS NULL) OR (\"ManuscriptId\" IS NULL AND \"PageTaskSubmissionId\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Annotations_ExactlyOneOwner",
                table: "Annotations");
        }
    }
}
