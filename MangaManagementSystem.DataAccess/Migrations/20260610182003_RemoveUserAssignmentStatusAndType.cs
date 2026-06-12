using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserAssignmentStatusAndType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS \"IX_UserAssignments_FromUserId_AssignmentType\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"UserAssignments\" DROP COLUMN IF EXISTS \"AssignmentType\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"UserAssignments\" DROP COLUMN IF EXISTS \"Status\";");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_UserAssignments_FromUserId_ToUserId\" " +
                "ON \"UserAssignments\" (\"FromUserId\", \"ToUserId\") " +
                "WHERE \"UnassignedAt\" IS NULL AND \"DeletedAt\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAssignments_FromUserId_ToUserId",
                table: "UserAssignments");

            migrationBuilder.AddColumn<string>(
                name: "AssignmentType",
                table: "UserAssignments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "UserAssignments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_FromUserId_AssignmentType",
                table: "UserAssignments",
                columns: new[] { "FromUserId", "AssignmentType" },
                unique: true,
                filter: "\"AssignmentType\" = 'TantouEditor' AND \"UnassignedAt\" IS NULL AND \"DeletedAt\" IS NULL");
        }
    }
}
