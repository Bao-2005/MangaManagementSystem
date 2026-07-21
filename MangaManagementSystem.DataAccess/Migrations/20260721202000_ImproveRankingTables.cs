using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ImproveRankingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VoteRecords_SeriesId",
                table: "VoteRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VoteRecords_Count",
                table: "VoteRecords");

            migrationBuilder.DropIndex(
                name: "IX_RankingSnapshots_SeriesId",
                table: "RankingSnapshots");

            migrationBuilder.AddColumn<int>(
                name: "ReaderCount",
                table: "RankingSnapshots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Score",
                table: "RankingSnapshots",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "VoteCount",
                table: "RankingSnapshots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "VoteRecordId",
                table: "RankingSnapshots",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoteRecords_Period_Status",
                table: "VoteRecords",
                columns: new[] { "Period", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_VoteRecords_SeriesId_Period",
                table: "VoteRecords",
                columns: new[] { "SeriesId", "Period" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VoteRecords_Count",
                table: "VoteRecords",
                sql: "\"ReaderCount\" >= 0 AND \"VoteCount\" >= 0 AND \"VoteCount\" <= \"ReaderCount\"");

            migrationBuilder.CreateIndex(
                name: "IX_RankingSnapshots_Period_RankNo",
                table: "RankingSnapshots",
                columns: new[] { "Period", "RankNo" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RankingSnapshots_Period_Score_VoteCount",
                table: "RankingSnapshots",
                columns: new[] { "Period", "Score", "VoteCount" });

            migrationBuilder.CreateIndex(
                name: "IX_RankingSnapshots_SeriesId_Period",
                table: "RankingSnapshots",
                columns: new[] { "SeriesId", "Period" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RankingSnapshots_VoteRecordId",
                table: "RankingSnapshots",
                column: "VoteRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_RankingSnapshots_VoteRecords_VoteRecordId",
                table: "RankingSnapshots",
                column: "VoteRecordId",
                principalTable: "VoteRecords",
                principalColumn: "VoteRecordId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RankingSnapshots_VoteRecords_VoteRecordId",
                table: "RankingSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_VoteRecords_Period_Status",
                table: "VoteRecords");

            migrationBuilder.DropIndex(
                name: "IX_VoteRecords_SeriesId_Period",
                table: "VoteRecords");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VoteRecords_Count",
                table: "VoteRecords");

            migrationBuilder.DropIndex(
                name: "IX_RankingSnapshots_Period_RankNo",
                table: "RankingSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_RankingSnapshots_Period_Score_VoteCount",
                table: "RankingSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_RankingSnapshots_SeriesId_Period",
                table: "RankingSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_RankingSnapshots_VoteRecordId",
                table: "RankingSnapshots");

            migrationBuilder.DropColumn(
                name: "ReaderCount",
                table: "RankingSnapshots");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "RankingSnapshots");

            migrationBuilder.DropColumn(
                name: "VoteCount",
                table: "RankingSnapshots");

            migrationBuilder.DropColumn(
                name: "VoteRecordId",
                table: "RankingSnapshots");

            migrationBuilder.CreateIndex(
                name: "IX_VoteRecords_SeriesId",
                table: "VoteRecords",
                column: "SeriesId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VoteRecords_Count",
                table: "VoteRecords",
                sql: "\"VoteCount\" <= \"ReaderCount\"");

            migrationBuilder.CreateIndex(
                name: "IX_RankingSnapshots_SeriesId",
                table: "RankingSnapshots",
                column: "SeriesId");
        }
    }
}
