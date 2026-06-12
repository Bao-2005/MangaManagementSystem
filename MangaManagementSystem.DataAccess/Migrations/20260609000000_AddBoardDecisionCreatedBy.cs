using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardDecisionCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "BoardDecisions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardDecisions_CreatedBy",
                table: "BoardDecisions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BoardDecisions_OpenSeriesProposal_SeriesId",
                table: "BoardDecisions",
                columns: new[] { "SeriesId", "DecisionType", "Status" },
                unique: true,
                filter: "\"DecisionType\" = 'SeriesProposal' AND \"Status\" = 'Open' AND \"DeletedAt\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardDecisions_Users_CreatedBy",
                table: "BoardDecisions",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardDecisions_Users_CreatedBy",
                table: "BoardDecisions");

            migrationBuilder.DropIndex(
                name: "IX_BoardDecisions_CreatedBy",
                table: "BoardDecisions");

            migrationBuilder.DropIndex(
                name: "IX_BoardDecisions_OpenSeriesProposal_SeriesId",
                table: "BoardDecisions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BoardDecisions");

        }
    }
}
