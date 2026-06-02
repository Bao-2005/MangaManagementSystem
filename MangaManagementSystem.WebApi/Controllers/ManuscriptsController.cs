using MangaManagementSystem.Business.Auth.Interfaces;
using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.DataAccess;
using MangaManagementSystem.Business.Constants;
using MangaManagementSystem.DataAccess.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MangaManagementSystem.WebApi.Controllers
{
    /// <summary>
    /// API endpoints cho Manuscript feature (Manuscript Submission + Editor Review + Chapter Publishing).
    ///
    /// Auth hiện tại dùng DevCurrentUserService — bỏ qua hoàn toàn khi test.
    /// Teammate implement JWT: tạo JwtCurrentUserService và đổi DI trong ServiceCollection.cs.
    /// </summary>
    [ApiController]
    [Route("api")]
    [Tags("Manuscripts")]
    public class ManuscriptsController : ControllerBase
    {
        private readonly IManuscriptService _manuscriptService;
        private readonly ICurrentUserService _currentUserService;

        public ManuscriptsController(
            IManuscriptService manuscriptService,
            ICurrentUserService currentUserService)
        {
            _manuscriptService = manuscriptService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// GET /api/manuscripts
        ///
        /// Lấy danh sách tất cả bản thảo trong hệ thống (tương ứng GET /manuscripts theo API Contract).
        /// - Tantou Editor / Admin: thấy tất cả.
        /// - Mangaka: chỉ thấy manuscripts của series mình phụ trách.
        /// </summary>
        [HttpGet("manuscripts")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Get all manuscripts",
            Description = "Lấy danh sách tất cả bản thảo trong hệ thống. Tantou Editor / Admin có thể xem tất cả. Mangaka chỉ xem được bản thảo thuộc series phụ trách.")]
        public async Task<IActionResult> GetAllManuscripts(
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var manuscripts = await _manuscriptService.GetAllAsync(
                    currentUserId.Value, cancellationToken);

                return Ok(manuscripts);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/chapters/{chapterId}/manuscripts
        ///
        /// Mangaka submit manuscript mới hoặc resubmit (tạo version mới).
        /// Enforce BR-67 (tất cả PageTask phải Approved), BR-72 (chỉ Mangaka owner series),
        /// BR-73 (versioning), BR-80 (không submit nếu đã Approved).
        /// Trả về 201 Created với manuscript vừa tạo.
        /// </summary>
        [HttpPost("chapters/{chapterId:guid}/manuscripts")]
        [Authorize(Roles = RoleConstants.Mangaka)]
        [SwaggerOperation(
            Summary = "Submit manuscript",
            Description = "Mangaka nộp bản thảo mới hoặc nộp lại (resubmit) bản thảo để tạo version mới. Áp dụng các luật: tất cả PageTask phải Approved, chỉ Mangaka sở hữu series mới được nộp, và không nộp nếu đã Approved.")]
        public async Task<IActionResult> SubmitManuscript(
            [FromRoute] Guid chapterId,
            [FromBody] SubmitManuscriptRequest request,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var result = await _manuscriptService.SubmitAsync(
                    chapterId, currentUserId.Value, request, cancellationToken);

                return CreatedAtAction(
                    nameof(GetManuscriptById),
                    new { manuscriptId = result.ManuscriptId },
                    new { success = true, data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/chapters/{chapterId}/manuscripts
        ///
        /// Lấy danh sách tất cả manuscript versions của một chapter (history).
        /// Mangaka owner, Tantou Editor assigned, hoặc Admin mới được xem.
        /// Trả về list sắp xếp theo VersionNo tăng dần.
        /// </summary>
        [HttpGet("chapters/{chapterId:guid}/manuscripts")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Get manuscripts by chapter",
            Description = "Lấy danh sách tất cả các phiên bản bản thảo của một chapter (lịch sử nộp). Chỉ Mangaka sở hữu, Tantou Editor được chỉ định hoặc Admin mới có quyền xem.")]
        public async Task<IActionResult> GetManuscriptsByChapter(
            [FromRoute] Guid chapterId,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var manuscripts = await _manuscriptService.GetListByChapterAsync(
                    chapterId, currentUserId.Value, cancellationToken);

                return Ok(new { manuscripts });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/manuscripts/{manuscriptId}
        ///
        /// Lấy chi tiết một manuscript theo ID.
        /// Mangaka owner, Tantou Editor assigned, hoặc Admin mới được xem.
        /// </summary>
        [HttpGet("manuscripts/{manuscriptId:guid}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Get manuscript by ID",
            Description = "Lấy thông tin chi tiết của một bản thảo theo ID. Quyền truy cập dành cho Mangaka sở hữu, Tantou Editor được chỉ định hoặc Admin.")]
        public async Task<IActionResult> GetManuscriptById(
            [FromRoute] Guid manuscriptId,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var manuscript = await _manuscriptService.GetByIdAsync(
                    manuscriptId, currentUserId.Value, cancellationToken);

                return Ok(manuscript);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/manuscripts/{manuscriptId}/start-review
        ///
        /// Tantou Editor bắt đầu review — chuyển status Submitted → Under Review.
        /// Enforce BR-74 (assigned editor), BR-75 (latest version), BR-76 (đúng flow).
        /// </summary>
        [HttpPost("manuscripts/{manuscriptId:guid}/start-review")]
        [Authorize(Roles = RoleConstants.TantouEditor)]
        [SwaggerOperation(
            Summary = "Start reviewing manuscript",
            Description = "Tantou Editor bắt đầu đánh giá bản thảo — chuyển trạng thái từ Submitted sang Under Review. Ràng buộc: Editor được chỉ định, phiên bản mới nhất, đúng luồng trạng thái.")]
        public async Task<IActionResult> StartReview(
            [FromRoute] Guid manuscriptId,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var result = await _manuscriptService.StartReviewAsync(
                    manuscriptId, currentUserId.Value, cancellationToken);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/manuscripts/{manuscriptId}/approve
        ///
        /// Tantou Editor approve manuscript — chuyển status Under Review → Approved.
        /// Đồng thời publish chapter (Chapter.Status = "Published").
        /// Enforce BR-74, BR-75, BR-80 (lock), BR-84 (completion 100%).
        /// </summary>
        [HttpPost("manuscripts/{manuscriptId:guid}/approve")]
        [Authorize(Roles = RoleConstants.TantouEditor)]
        [SwaggerOperation(
            Summary = "Approve manuscript",
            Description = "Tantou Editor duyệt bản thảo — chuyển trạng thái từ Under Review sang Approved và xuất bản chapter (Published). Ràng buộc: Editor được chỉ định, phiên bản mới nhất, hoàn thành 100%.")]
        public async Task<IActionResult> ApproveManuscript(
            [FromRoute] Guid manuscriptId,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var result = await _manuscriptService.ApproveAsync(
                    manuscriptId, currentUserId.Value, cancellationToken);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/manuscripts/{manuscriptId}/request-revision
        ///
        /// Tantou Editor yêu cầu sửa — chuyển status Under Review → Revision Required.
        /// Enforce BR-74, BR-75, BR-77 (cần annotation + feedback), BR-83 (max 3 rounds).
        /// </summary>
        [HttpPost("manuscripts/{manuscriptId:guid}/request-revision")]
        [Authorize(Roles = RoleConstants.TantouEditor)]
        [SwaggerOperation(
            Summary = "Request manuscript revision",
            Description = "Tantou Editor yêu cầu sửa đổi bản thảo — chuyển trạng thái sang Revision Required. Yêu cầu phải có ít nhất 1 annotation và feedback, tối đa 3 vòng sửa đổi.")]
        public async Task<IActionResult> RequestRevision(
            [FromRoute] Guid manuscriptId,
            [FromBody] RequestRevisionRequest request,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var result = await _manuscriptService.RequestRevisionAsync(
                    manuscriptId, currentUserId.Value, request, cancellationToken);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/dev/seed-data
        /// 
        /// Endpoint tiện ích tự động chèn dữ liệu mẫu vào Database phục vụ cho việc kiểm thử tay.
        /// Chèn đầy đủ thông tin: Roles, Users (Mangaka, Editor, Assistant), Series (Active), 
        /// Chapter, Manuscript ban đầu, và PageTask (Approved) để thỏa mãn toàn bộ Business Rules.
        /// </summary>
        [HttpPost("dev/seed-data")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Seed development data",
            Description = "API tiện ích tự động chèn dữ liệu mẫu (Roles, Users, Series, Chapter, PageTask, Manuscript) vào Database phục vụ cho việc kiểm thử.")]
        public async Task<IActionResult> SeedDevData(
            [FromServices] MangaManagement.DataAccess.DbContexts.MangaDbContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var chapterId = new Guid("55555555-5555-5555-5555-555555555555");
                var seriesId = new Guid("44444444-4444-4444-4444-444444444444");

                // --- DỌN DẸP DỮ LIỆU CŨ PHỤC VỤ RESET ---
                // 1. Xóa các Annotations của các Manuscripts thuộc chapter test này
                var oldManuscriptIds = await context.Manuscripts
                    .Where(m => m.ChapterId == chapterId)
                    .Select(m => m.ManuscriptId)
                    .ToListAsync(cancellationToken);

                var oldAnnotations = await context.Annotations
                    .Where(a => oldManuscriptIds.Contains(a.ManuscriptId))
                    .ToListAsync(cancellationToken);
                if (oldAnnotations.Any())
                {
                    context.Annotations.RemoveRange(oldAnnotations);
                }

                // 2. Xóa các PageTasks của chapter test này
                var oldPageTasks = await context.PageTasks
                    .Where(pt => pt.ChapterId == chapterId)
                    .ToListAsync(cancellationToken);
                if (oldPageTasks.Any())
                {
                    context.PageTasks.RemoveRange(oldPageTasks);
                }

                // 3. Xóa các Manuscripts cũ của chapter test này
                var oldManuscripts = await context.Manuscripts
                    .Where(m => m.ChapterId == chapterId)
                    .ToListAsync(cancellationToken);
                if (oldManuscripts.Any())
                {
                    context.Manuscripts.RemoveRange(oldManuscripts);
                }

                // 4. Reset trạng thái Chapter nếu đã tồn tại
                var existingChapter = await context.Chapters.FirstOrDefaultAsync(c => c.ChapterId == chapterId, cancellationToken);
                if (existingChapter != null)
                {
                    existingChapter.Status = "Draft";
                    context.Chapters.Update(existingChapter);
                }

                // 5. Reset trạng thái Series nếu đã tồn tại
                var existingSeries = await context.Series.FirstOrDefaultAsync(s => s.SeriesId == seriesId, cancellationToken);
                if (existingSeries != null)
                {
                    existingSeries.Status = "Active";
                    context.Series.Update(existingSeries);
                }

                await context.SaveChangesAsync(cancellationToken);
                // --- KẾT THÚC DỌN DẸP ---

                // 1. Tạo các Roles nếu chưa tồn tại
                var mangakaRoleId = Guid.NewGuid();
                var editorRoleId = Guid.NewGuid();
                var assistantRoleId = Guid.NewGuid();

                var roleMangaka = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Mangaka", cancellationToken);
                if (roleMangaka == null)
                {
                    roleMangaka = new Role { RoleId = mangakaRoleId, RoleName = "Mangaka" };
                    await context.Roles.AddAsync(roleMangaka, cancellationToken);
                }
                else mangakaRoleId = roleMangaka.RoleId;

                var roleEditor = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == RoleConstants.TantouEditor, cancellationToken);
                if (roleEditor == null)
                {
                    roleEditor = new Role { RoleId = editorRoleId, RoleName = RoleConstants.TantouEditor };
                    await context.Roles.AddAsync(roleEditor, cancellationToken);
                }
                else editorRoleId = roleEditor.RoleId;

                var roleAssistant = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Assistant", cancellationToken);
                if (roleAssistant == null)
                {
                    roleAssistant = new Role { RoleId = assistantRoleId, RoleName = "Assistant" };
                    await context.Roles.AddAsync(roleAssistant, cancellationToken);
                }
                else assistantRoleId = roleAssistant.RoleId;

                await context.SaveChangesAsync(cancellationToken);

                // 2. Tạo Users mẫu nếu chưa tồn tại
                var mangakaId = new Guid("11111111-1111-1111-1111-111111111111"); // DevUserId
                var editorId = new Guid("22222222-2222-2222-2222-222222222222");
                var assistantId = new Guid("33333333-3333-3333-3333-333333333333");

                var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
                var dummyUser = new User();

                var userMangaka = await context.Users.FirstOrDefaultAsync(u => u.UserId == mangakaId, cancellationToken);
                if (userMangaka == null)
                {
                    userMangaka = new User
                    {
                        UserId = mangakaId,
                        UserName = "dev_mangaka",
                        Email = "mangaka@manga.com",
                        DisplayName = "Họa sĩ Mangaka mẫu (Dev User)",
                        PasswordHash = passwordHasher.HashPassword(dummyUser, "Test@1234"),
                        Status = "Active",
                        RoleId = mangakaRoleId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.Users.AddAsync(userMangaka, cancellationToken);
                }

                var userEditor = await context.Users.FirstOrDefaultAsync(u => u.UserId == editorId, cancellationToken);
                if (userEditor == null)
                {
                    userEditor = new User
                    {
                        UserId = editorId,
                        UserName = "dev_editor",
                        Email = "editor@manga.com",
                        DisplayName = "Tantou Editor Biên tập viên",
                        PasswordHash = passwordHasher.HashPassword(dummyUser, "Test@1234"),
                        Status = "Active",
                        RoleId = editorRoleId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.Users.AddAsync(userEditor, cancellationToken);
                }

                var userAssistant = await context.Users.FirstOrDefaultAsync(u => u.UserId == assistantId, cancellationToken);
                if (userAssistant == null)
                {
                    userAssistant = new User
                    {
                        UserId = assistantId,
                        UserName = "dev_assistant",
                        Email = "assistant@manga.com",
                        DisplayName = "Trợ lý thiết kế (Assistant)",
                        PasswordHash = passwordHasher.HashPassword(dummyUser, "Test@1234"),
                        Status = "Active",
                        RoleId = assistantRoleId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.Users.AddAsync(userAssistant, cancellationToken);
                }

                await context.SaveChangesAsync(cancellationToken);

                // 2.5 Tạo FileAssets mẫu nếu chưa tồn tại
                var previewFileAssetId = new Guid("88888888-8888-8888-8888-888888888888");
                var sourceFileAssetId = new Guid("99999999-9999-9999-9999-999999999999");

                var previewAsset = await context.FileAssets.FirstOrDefaultAsync(fa => fa.FileAssetId == previewFileAssetId, cancellationToken);
                if (previewAsset == null)
                {
                    previewAsset = new FileAsset
                    {
                        FileAssetId = previewFileAssetId,
                        BucketName = "manga-previews",
                        ObjectPath = "previews/test-chapter-1.pdf",
                        OriginalFileName = "chapter_1_sketch.pdf",
                        StoredFileName = "stored_test_chapter_1_sketch.pdf",
                        MimeType = "application/pdf",
                        FileSizeBytes = 1048576,
                        FileCategory = "Preview",
                        UploadedBy = mangakaId,
                        UploadedAt = DateTime.UtcNow
                    };
                    await context.FileAssets.AddAsync(previewAsset, cancellationToken);
                }

                var sourceAsset = await context.FileAssets.FirstOrDefaultAsync(fa => fa.FileAssetId == sourceFileAssetId, cancellationToken);
                if (sourceAsset == null)
                {
                    sourceAsset = new FileAsset
                    {
                        FileAssetId = sourceFileAssetId,
                        BucketName = "manga-sources",
                        ObjectPath = "sources/test-chapter-1.zip",
                        OriginalFileName = "chapter_1_source.zip",
                        StoredFileName = "stored_test_chapter_1_source.zip",
                        MimeType = "application/zip",
                        FileSizeBytes = 52428800,
                        FileCategory = "Source",
                        UploadedBy = mangakaId,
                        UploadedAt = DateTime.UtcNow
                    };
                    await context.FileAssets.AddAsync(sourceAsset, cancellationToken);
                }
                await context.SaveChangesAsync(cancellationToken);

                // 3. Tạo Series mẫu
                var series = await context.Series.FirstOrDefaultAsync(s => s.SeriesId == seriesId, cancellationToken);
                if (series == null)
                {
                    series = new Series
                    {
                        SeriesId = seriesId,
                        Title = "Kiếm Sĩ Diệt Quỷ (Test Series)",
                        Genre = "Action, Adventure, Shounen",
                        PublicationType = "Weekly",
                        Status = "Active", // Phải Active
                        MangakaId = mangakaId,
                        TantouEditorId = editorId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.Series.AddAsync(series, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }

                // 4. Tạo Chapter mẫu
                var chapter = await context.Chapters.FirstOrDefaultAsync(c => c.ChapterId == chapterId, cancellationToken);
                if (chapter == null)
                {
                    chapter = new Chapter
                    {
                        ChapterId = chapterId,
                        SeriesId = seriesId,
                        ChapterNo = 1,
                        Title = "Chương 1: Bình minh định mệnh",
                        TotalPages = 20,
                        Status = "Draft",
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.Chapters.AddAsync(chapter, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }

                // 5. Tạo Manuscript gốc (Draft version 0) để thỏa mãn khóa ngoại cho PageTask
                var manuscriptV0Id = new Guid("77777777-7777-7777-7777-777777777777");
                var manuscriptV0 = await context.Manuscripts.FirstOrDefaultAsync(m => m.ManuscriptId == manuscriptV0Id, cancellationToken);
                if (manuscriptV0 == null)
                {
                    manuscriptV0 = new Manuscript
                    {
                        ManuscriptId = manuscriptV0Id,
                        ChapterId = chapterId,
                        VersionNo = 0,
                        Status = "Draft",
                        SubmittedBy = mangakaId,
                        SubmittedAt = DateTime.UtcNow.AddHours(-1),
                        RevisionCount = 0,
                        Notes = "Bản nháp ban đầu"
                    };
                    await context.Manuscripts.AddAsync(manuscriptV0, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }

                // 6. Tạo PageTask (Approved) để thỏa mãn BR-67
                var pageTaskId = new Guid("66666666-6666-6666-6666-666666666666");
                var pageTask = await context.PageTasks.FirstOrDefaultAsync(pt => pt.PageTaskId == pageTaskId, cancellationToken);
                if (pageTask == null)
                {
                    pageTask = new PageTask
                    {
                        PageTaskId = pageTaskId,
                        ChapterId = chapterId,
                        ManuscriptId = manuscriptV0Id,
                        AssistantId = assistantId,
                        PageStart = 1,
                        PageEnd = 20,
                        TaskType = "Drafting",
                        Description = "Hỗ trợ vẽ nét phác thảo",
                        Status = MangaManagementSystem.DataAccess.Entities.Enums.PageTaskStatus.Approved, // Phải Approved để pass BR-67
                        ApprovedAt = DateTime.UtcNow.AddMinutes(-30),
                        CreatedAt = DateTime.UtcNow.AddHours(-1)
                    };
                    await context.PageTasks.AddAsync(pageTask, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }

                return Ok(new
                {
                    success = true,
                    message = "Đã khởi tạo thành công toàn bộ dữ liệu mẫu kiểm thử trong Database!",
                    data = new
                    {
                        mangakaId = mangakaId,
                        editorId = editorId,
                        assistantId = assistantId,
                        seriesId = seriesId,
                        chapterId = chapterId,
                        pageTaskId = pageTaskId,
                        initialManuscriptV0Id = manuscriptV0Id,
                        previewFileAssetId = previewFileAssetId,
                        sourceFileAssetId = sourceFileAssetId
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi seed data: " + ex.Message, detail = ex.InnerException?.Message });
            }
        }
    }
}
