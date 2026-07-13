using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class configindexseriepagenoinConfigureProposalPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProposalPages_SeriesId_PageNo",
                table: "ProposalPages");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalPages_SeriesId_PageNo",
                table: "ProposalPages",
                columns: new[] { "SeriesId", "PageNo" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProposalPages_SeriesId_PageNo",
                table: "ProposalPages");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalPages_SeriesId_PageNo",
                table: "ProposalPages",
                columns: new[] { "SeriesId", "PageNo" },
                unique: true);
        }
    }
}
