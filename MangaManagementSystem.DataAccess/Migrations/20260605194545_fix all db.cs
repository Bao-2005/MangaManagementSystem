using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fixalldb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Annotations_ChapterPages_ChapterPageId",
                table: "Annotations");

            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Series_SeriesId",
                table: "Chapters");

            migrationBuilder.DropForeignKey(
                name: "FK_FileAssets_Users_UploadedBy",
                table: "FileAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_Manuscripts_Chapters_ChapterId",
                table: "Manuscripts");

            migrationBuilder.DropForeignKey(
                name: "FK_Manuscripts_FileAssets_PreviewFileAssetId",
                table: "Manuscripts");

            migrationBuilder.DropForeignKey(
                name: "FK_Manuscripts_FileAssets_SourceFileAssetId",
                table: "Manuscripts");

            migrationBuilder.DropForeignKey(
                name: "FK_PageTaskSubmissions_PageTasks_PageTaskId",
                table: "PageTaskSubmissions");

            migrationBuilder.DropTable(
                name: "ChapterPages");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserName",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserAssignments_FromUserId",
                table: "UserAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserAssignments_ToUserId",
                table: "UserAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Manuscripts_PreviewFileAssetId",
                table: "Manuscripts");

            migrationBuilder.DropIndex(
                name: "IX_FileAssets_ObjectPath",
                table: "FileAssets");

            migrationBuilder.DropIndex(
                name: "IX_FileAssets_UploadedBy",
                table: "FileAssets");

            migrationBuilder.DropIndex(
                name: "IX_Annotations_ChapterPageId",
                table: "Annotations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "PreviewFileAssetId",
                table: "Manuscripts");

            migrationBuilder.DropColumn(
                name: "FileCategory",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "UploadedBy",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "ChapterPageId",
                table: "Annotations");

            migrationBuilder.RenameColumn(
                name: "TantouEditorId",
                table: "Series",
                newName: "SourceZipFileAssetId");

            migrationBuilder.RenameColumn(
                name: "SourceFileAssetId",
                table: "Manuscripts",
                newName: "ReviewedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Manuscripts_SourceFileAssetId",
                table: "Manuscripts",
                newName: "IX_Manuscripts_ReviewedBy");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenHash",
                table: "Users",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "UserAssignments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedAt",
                table: "UserAssignments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "AssignmentType",
                table: "UserAssignments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Series",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Series",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<decimal>(
                name: "RankingScore",
                table: "Series",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "Series",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Series",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Synopsis",
                table: "Series",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "Roles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "PageTaskSubmissions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PageTasks",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "PageTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "Manuscripts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Feedback",
                table: "Manuscripts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "Manuscripts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RevisionCount",
                table: "Manuscripts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "FileAssets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "FileAssets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StorageProvider",
                table: "FileAssets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Chapters",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Chapters",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Annotations",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateTable(
                name: "BoardDecisions",
                columns: table => new
                {
                    BoardDecisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VotingDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardDecisions", x => x.BoardDecisionId);
                    table.ForeignKey(
                        name: "FK_BoardDecisions_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Escalations",
                columns: table => new
                {
                    EscalationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResolvedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escalations", x => x.EscalationId);
                    table.ForeignKey(
                        name: "FK_Escalations_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Escalations_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Escalations_Users_ResolvedBy",
                        column: x => x.ResolvedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    GenreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.GenreId);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "ProposalPages",
                columns: table => new
                {
                    ProposalPageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageNo = table.Column<int>(type: "int", nullable: false),
                    PreviewFileAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalPages", x => x.ProposalPageId);
                    table.ForeignKey(
                        name: "FK_ProposalPages_FileAssets_PreviewFileAssetId",
                        column: x => x.PreviewFileAssetId,
                        principalTable: "FileAssets",
                        principalColumn: "FileAssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProposalPages_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RankingSnapshots",
                columns: table => new
                {
                    RankingSnapshotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RankNo = table.Column<int>(type: "int", nullable: false),
                    IsBottom20Percent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingSnapshots", x => x.RankingSnapshotId);
                    table.ForeignKey(
                        name: "FK_RankingSnapshots_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VoteRecords",
                columns: table => new
                {
                    VoteRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReaderCount = table.Column<int>(type: "int", nullable: false),
                    VoteCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConfirmedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteRecords", x => x.VoteRecordId);
                    table.CheckConstraint("CK_VoteRecords_Count", "[VoteCount] <= [ReaderCount]");
                    table.ForeignKey(
                        name: "FK_VoteRecords_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoteRecords_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BoardVotes",
                columns: table => new
                {
                    BoardVoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    BoardDecisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoteValue = table.Column<bool>(type: "bit", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardVotes", x => x.BoardVoteId);
                    table.ForeignKey(
                        name: "FK_BoardVotes_BoardDecisions_BoardDecisionId",
                        column: x => x.BoardDecisionId,
                        principalTable: "BoardDecisions",
                        principalColumn: "BoardDecisionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BoardVotes_Users_VoterId",
                        column: x => x.VoterId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeriesGenres",
                columns: table => new
                {
                    SeriesGenreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesGenres", x => x.SeriesGenreId);
                    table.ForeignKey(
                        name: "FK_SeriesGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "GenreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeriesGenres_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    UserNotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => x.UserNotificationId);
                    table.ForeignKey(
                        name: "FK_UserNotifications_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_FromUserId_AssignmentType",
                table: "UserAssignments",
                columns: new[] { "FromUserId", "AssignmentType" },
                unique: true,
                filter: "[AssignmentType] = 'TantouEditor' AND [UnassignedAt] IS NULL AND [DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_ToUserId",
                table: "UserAssignments",
                column: "ToUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAssignments_NotSelf",
                table: "UserAssignments",
                sql: "[FromUserId] <> [ToUserId]");

            migrationBuilder.CreateIndex(
                name: "IX_Series_MangakaId",
                table: "Series",
                column: "MangakaId");

            migrationBuilder.CreateIndex(
                name: "IX_Series_SourceZipFileAssetId",
                table: "Series",
                column: "SourceZipFileAssetId",
                unique: true,
                filter: "[SourceZipFileAssetId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PageTasks_UserId",
                table: "PageTasks",
                column: "UserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PageTasks_PageRange",
                table: "PageTasks",
                sql: "[PageStart] <= [PageEnd]");

            migrationBuilder.CreateIndex(
                name: "IX_BoardDecisions_SeriesId",
                table: "BoardDecisions",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardVotes_BoardDecisionId_VoterId",
                table: "BoardVotes",
                columns: new[] { "BoardDecisionId", "VoterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardVotes_VoterId",
                table: "BoardVotes",
                column: "VoterId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalations_CreatedBy",
                table: "Escalations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Escalations_ResolvedBy",
                table: "Escalations",
                column: "ResolvedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Escalations_SeriesId",
                table: "Escalations",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Title",
                table: "Genres",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProposalPages_PreviewFileAssetId",
                table: "ProposalPages",
                column: "PreviewFileAssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProposalPages_SeriesId_PageNo",
                table: "ProposalPages",
                columns: new[] { "SeriesId", "PageNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankingSnapshots_SeriesId",
                table: "RankingSnapshots",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesGenres_GenreId",
                table: "SeriesGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesGenres_SeriesId_GenreId",
                table: "SeriesGenres",
                columns: new[] { "SeriesId", "GenreId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_NotificationId",
                table: "UserNotifications",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId_NotificationId",
                table: "UserNotifications",
                columns: new[] { "UserId", "NotificationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoteRecords_ConfirmedBy",
                table: "VoteRecords",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VoteRecords_SeriesId",
                table: "VoteRecords",
                column: "SeriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Series_SeriesId",
                table: "Chapters",
                column: "SeriesId",
                principalTable: "Series",
                principalColumn: "SeriesId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Manuscripts_Chapters_ChapterId",
                table: "Manuscripts",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Manuscripts_Users_ReviewedBy",
                table: "Manuscripts",
                column: "ReviewedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageTasks_Users_UserId",
                table: "PageTasks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PageTaskSubmissions_PageTasks_PageTaskId",
                table: "PageTaskSubmissions",
                column: "PageTaskId",
                principalTable: "PageTasks",
                principalColumn: "PageTaskId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Series_FileAssets_SourceZipFileAssetId",
                table: "Series",
                column: "SourceZipFileAssetId",
                principalTable: "FileAssets",
                principalColumn: "FileAssetId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Series_Users_MangakaId",
                table: "Series",
                column: "MangakaId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Series_SeriesId",
                table: "Chapters");

            migrationBuilder.DropForeignKey(
                name: "FK_Manuscripts_Chapters_ChapterId",
                table: "Manuscripts");

            migrationBuilder.DropForeignKey(
                name: "FK_Manuscripts_Users_ReviewedBy",
                table: "Manuscripts");

            migrationBuilder.DropForeignKey(
                name: "FK_PageTasks_Users_UserId",
                table: "PageTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_PageTaskSubmissions_PageTasks_PageTaskId",
                table: "PageTaskSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Series_FileAssets_SourceZipFileAssetId",
                table: "Series");

            migrationBuilder.DropForeignKey(
                name: "FK_Series_Users_MangakaId",
                table: "Series");

            migrationBuilder.DropTable(
                name: "BoardVotes");

            migrationBuilder.DropTable(
                name: "Escalations");

            migrationBuilder.DropTable(
                name: "ProposalPages");

            migrationBuilder.DropTable(
                name: "RankingSnapshots");

            migrationBuilder.DropTable(
                name: "SeriesGenres");

            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropTable(
                name: "VoteRecords");

            migrationBuilder.DropTable(
                name: "BoardDecisions");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_UserAssignments_FromUserId_AssignmentType",
                table: "UserAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserAssignments_ToUserId",
                table: "UserAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAssignments_NotSelf",
                table: "UserAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Series_MangakaId",
                table: "Series");

            migrationBuilder.DropIndex(
                name: "IX_Series_SourceZipFileAssetId",
                table: "Series");

            migrationBuilder.DropIndex(
                name: "IX_PageTasks_UserId",
                table: "PageTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PageTasks_PageRange",
                table: "PageTasks");

            migrationBuilder.DropColumn(
                name: "AssignmentType",
                table: "UserAssignments");

            migrationBuilder.DropColumn(
                name: "RankingScore",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "Synopsis",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PageTasks");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "Manuscripts");

            migrationBuilder.DropColumn(
                name: "RevisionCount",
                table: "Manuscripts");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "StorageProvider",
                table: "FileAssets");

            migrationBuilder.RenameColumn(
                name: "SourceZipFileAssetId",
                table: "Series",
                newName: "TantouEditorId");

            migrationBuilder.RenameColumn(
                name: "ReviewedBy",
                table: "Manuscripts",
                newName: "SourceFileAssetId");

            migrationBuilder.RenameIndex(
                name: "IX_Manuscripts_ReviewedBy",
                table: "Manuscripts",
                newName: "IX_Manuscripts_SourceFileAssetId");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenHash",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "UserAssignments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedAt",
                table: "UserAssignments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Series",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Series",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Series",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "Roles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "PageTaskSubmissions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PageTasks",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "Manuscripts",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<string>(
                name: "Feedback",
                table: "Manuscripts",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviewFileAssetId",
                table: "Manuscripts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileCategory",
                table: "FileAssets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "FileAssets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UploadedBy",
                table: "FileAssets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Chapters",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Chapters",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Annotations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<Guid>(
                name: "ChapterPageId",
                table: "Annotations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ChapterPages",
                columns: table => new
                {
                    ChapterPageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageFileAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManuscriptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PageNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterPages", x => x.ChapterPageId);
                    table.ForeignKey(
                        name: "FK_ChapterPages_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChapterPages_FileAssets_ImageFileAssetId",
                        column: x => x.ImageFileAssetId,
                        principalTable: "FileAssets",
                        principalColumn: "FileAssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChapterPages_Manuscripts_ManuscriptId",
                        column: x => x.ManuscriptId,
                        principalTable: "Manuscripts",
                        principalColumn: "ManuscriptId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_FromUserId",
                table: "UserAssignments",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignments_ToUserId",
                table: "UserAssignments",
                column: "ToUserId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Manuscripts_PreviewFileAssetId",
                table: "Manuscripts",
                column: "PreviewFileAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_FileAssets_ObjectPath",
                table: "FileAssets",
                column: "ObjectPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileAssets_UploadedBy",
                table: "FileAssets",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_ChapterPageId",
                table: "Annotations",
                column: "ChapterPageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPages_ChapterId",
                table: "ChapterPages",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPages_ImageFileAssetId",
                table: "ChapterPages",
                column: "ImageFileAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPages_ManuscriptId_PageNo",
                table: "ChapterPages",
                columns: new[] { "ManuscriptId", "PageNo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Annotations_ChapterPages_ChapterPageId",
                table: "Annotations",
                column: "ChapterPageId",
                principalTable: "ChapterPages",
                principalColumn: "ChapterPageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Series_SeriesId",
                table: "Chapters",
                column: "SeriesId",
                principalTable: "Series",
                principalColumn: "SeriesId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FileAssets_Users_UploadedBy",
                table: "FileAssets",
                column: "UploadedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Manuscripts_Chapters_ChapterId",
                table: "Manuscripts",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "ChapterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Manuscripts_FileAssets_PreviewFileAssetId",
                table: "Manuscripts",
                column: "PreviewFileAssetId",
                principalTable: "FileAssets",
                principalColumn: "FileAssetId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Manuscripts_FileAssets_SourceFileAssetId",
                table: "Manuscripts",
                column: "SourceFileAssetId",
                principalTable: "FileAssets",
                principalColumn: "FileAssetId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PageTaskSubmissions_PageTasks_PageTaskId",
                table: "PageTaskSubmissions",
                column: "PageTaskId",
                principalTable: "PageTasks",
                principalColumn: "PageTaskId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
