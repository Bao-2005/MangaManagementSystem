using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardDecisionExtensionAndSpecialDecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExtendedAt",
                table: "BoardDecisions",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExtendedBy",
                table: "BoardDecisions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExtensionCount",
                table: "BoardDecisions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExtensionReason",
                table: "BoardDecisions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SpecialDecisionAt",
                table: "BoardDecisions",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecialDecisionBy",
                table: "BoardDecisions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialDecisionReason",
                table: "BoardDecisions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtendedAt",
                table: "BoardDecisions");

            migrationBuilder.DropColumn(
                name: "ExtendedBy",
                table: "BoardDecisions");

            migrationBuilder.DropColumn(
                name: "ExtensionCount",
                table: "BoardDecisions");

            migrationBuilder.DropColumn(
                name: "ExtensionReason",
                table: "BoardDecisions");

            migrationBuilder.DropColumn(
                name: "SpecialDecisionAt",
                table: "BoardDecisions");

            migrationBuilder.DropColumn(
                name: "SpecialDecisionBy",
                table: "BoardDecisions");

            migrationBuilder.DropColumn(
                name: "SpecialDecisionReason",
                table: "BoardDecisions");
        }
    }
}
