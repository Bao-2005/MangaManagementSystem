using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueOpenEscalationIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Escalations_Type_EntityType_EntityId",
                table: "Escalations",
                columns: new[] { "Type", "EntityType", "EntityId" },
                unique: true,
                filter: "\"Status\" IN ('Open', 'InReview') AND \"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Escalations_Type_EntityType_EntityId",
                table: "Escalations");
        }
    }
}
