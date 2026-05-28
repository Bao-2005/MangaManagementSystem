using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaManagementSystem.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    SeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MangakaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TantouEditorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PublicationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.SeriesId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterNo = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TotalPages = table.Column<int>(type: "int", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmissionDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.ChapterId);
                    table.ForeignKey(
                        name: "FK_Chapters_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileAssets",
                columns: table => new
                {
                    FileAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BucketName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ObjectPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAssets", x => x.FileAssetId);
                    table.ForeignKey(
                        name: "FK_FileAssets_Users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Manuscripts",
                columns: table => new
                {
                    ManuscriptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    PreviewFileAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceFileAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manuscripts", x => x.ManuscriptId);
                    table.ForeignKey(
                        name: "FK_Manuscripts_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Manuscripts_FileAssets_PreviewFileAssetId",
                        column: x => x.PreviewFileAssetId,
                        principalTable: "FileAssets",
                        principalColumn: "FileAssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Manuscripts_FileAssets_SourceFileAssetId",
                        column: x => x.SourceFileAssetId,
                        principalTable: "FileAssets",
                        principalColumn: "FileAssetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChapterPages",
                columns: table => new
                {
                    ChapterPageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManuscriptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageNo = table.Column<int>(type: "int", nullable: false),
                    ImageFileAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "PageTasks",
                columns: table => new
                {
                    PageTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManuscriptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssistantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageStart = table.Column<int>(type: "int", nullable: false),
                    PageEnd = table.Column<int>(type: "int", nullable: false),
                    TaskType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageTasks", x => x.PageTaskId);
                    table.ForeignKey(
                        name: "FK_PageTasks_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "ChapterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageTasks_Manuscripts_ManuscriptId",
                        column: x => x.ManuscriptId,
                        principalTable: "Manuscripts",
                        principalColumn: "ManuscriptId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageTasks_Users_AssistantId",
                        column: x => x.AssistantId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Annotations",
                columns: table => new
                {
                    AnnotationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManuscriptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterPageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageNo = table.Column<int>(type: "int", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annotations", x => x.AnnotationId);
                    table.ForeignKey(
                        name: "FK_Annotations_ChapterPages_ChapterPageId",
                        column: x => x.ChapterPageId,
                        principalTable: "ChapterPages",
                        principalColumn: "ChapterPageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Annotations_Manuscripts_ManuscriptId",
                        column: x => x.ManuscriptId,
                        principalTable: "Manuscripts",
                        principalColumn: "ManuscriptId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Annotations_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PageTaskSubmissions",
                columns: table => new
                {
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    SubmittedFileAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageTaskSubmissions", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_PageTaskSubmissions_FileAssets_SubmittedFileAssetId",
                        column: x => x.SubmittedFileAssetId,
                        principalTable: "FileAssets",
                        principalColumn: "FileAssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageTaskSubmissions_PageTasks_PageTaskId",
                        column: x => x.PageTaskId,
                        principalTable: "PageTasks",
                        principalColumn: "PageTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_AuthorId",
                table: "Annotations",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_ChapterPageId",
                table: "Annotations",
                column: "ChapterPageId");

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_ManuscriptId",
                table: "Annotations",
                column: "ManuscriptId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_SeriesId_ChapterNo",
                table: "Chapters",
                columns: new[] { "SeriesId", "ChapterNo" },
                unique: true);

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
                name: "IX_Manuscripts_ChapterId_VersionNo",
                table: "Manuscripts",
                columns: new[] { "ChapterId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Manuscripts_PreviewFileAssetId",
                table: "Manuscripts",
                column: "PreviewFileAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Manuscripts_SourceFileAssetId",
                table: "Manuscripts",
                column: "SourceFileAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_PageTasks_AssistantId",
                table: "PageTasks",
                column: "AssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_PageTasks_ChapterId",
                table: "PageTasks",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_PageTasks_ManuscriptId",
                table: "PageTasks",
                column: "ManuscriptId");

            migrationBuilder.CreateIndex(
                name: "IX_PageTaskSubmissions_PageTaskId_VersionNo",
                table: "PageTaskSubmissions",
                columns: new[] { "PageTaskId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageTaskSubmissions_SubmittedFileAssetId",
                table: "PageTaskSubmissions",
                column: "SubmittedFileAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Annotations");

            migrationBuilder.DropTable(
                name: "PageTaskSubmissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "ChapterPages");

            migrationBuilder.DropTable(
                name: "PageTasks");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Manuscripts");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "FileAssets");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
