using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTantouEditorRoleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Roles SET RoleName = 'Tantou Editor' WHERE RoleName = 'TANTOU_EDITOR';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Roles SET RoleName = 'TANTOU_EDITOR' WHERE RoleName = 'Tantou Editor';");
        }
    }
}
