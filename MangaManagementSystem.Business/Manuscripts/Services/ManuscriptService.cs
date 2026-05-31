using MangaManagement.DataAccess.DbContexts;
using MangaManagementSystem.Business.Manuscripts.DTOs;
using MangaManagementSystem.Business.Manuscripts.Interfaces;
using MangaManagementSystem.Business.Auth.Interfaces;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.Manuscripts.Services
{
    public class ManuscriptService : IManuscriptService
    {
        private readonly IManuscriptRepository _manuscriptRepository;
        private readonly IAnnotationRepository _annotationRepository;
        private readonly MangaDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        // Role name constants — khớp với giá trị trong bảng Roles
        private const string RoleTantouEditor = "TANTOU_EDITOR";
        private const string RoleMangaka = "MANGAKA";
        private const string RoleAdmin = "ADMIN";

        // Manuscript status constants (phụ lục roadmap)
        private const string StatusSubmitted = "Submitted";
        private const string StatusUnderReview = "Under Review";
        private const string StatusRevisionRequired = "Revision Required";
        private const string StatusApproved = "Approved";

        // Chapter status constants (phụ lục roadmap)
        private const string ChapterStatusSubmitted = "Submitted";
        private const string ChapterStatusRevisionRequired = "Revision Required";
        private const string ChapterStatusPublished = "Published";

        // PageTask status constants
        private const string PageTaskStatusApproved = "Approved";

        // Series status constants
        private const string SeriesStatusActive = "Active";

        // Revision limit (BR-83)
        private const int MaxRevisionRounds = 3;

        public ManuscriptService(
            IManuscriptRepository manuscriptRepository,
            IAnnotationRepository annotationRepository,
            MangaDbContext context,
            ICurrentUserService currentUserService)
        {
            _manuscriptRepository = manuscriptRepository;
            _annotationRepository = annotationRepository;
            _context = context;
            _currentUserService = currentUserService;
        }

        /// <inheritdoc />
        public async Task<ManuscriptResponse> SubmitAsync(
            Guid chapterId,
            Guid currentUserId,
            SubmitManuscriptRequest request,
            CancellationToken ct = default)
        {
            // 1. Kiểm tra role: phải là Mangaka (BR-72)
            if (!_currentUserService.BypassAuthorization)
            {
                var userRoles = await GetUserRolesAsync(currentUserId, ct);
                if (!userRoles.Contains(RoleMangaka))
                    throw new UnauthorizedAccessException("Chỉ Mangaka mới được submit manuscript.");
            }

            // 2. Load Chapter + Series — 404 nếu không tìm thấy
            var chapter = await _context.Chapters
                .Include(c => c.Series)
                .FirstOrDefaultAsync(c => c.ChapterId == chapterId, ct);

            if (chapter == null)
                throw new KeyNotFoundException($"Không tìm thấy chapter với ID: {chapterId}");

            var series = chapter.Series;

            // 3. Check object-level: user phải là Mangaka owner của series (BR-72)
            if (!_currentUserService.BypassAuthorization && series.MangakaId != currentUserId)
                throw new UnauthorizedAccessException("Bạn không phải Mangaka phụ trách series này.");

            // 4. Check series status == "Active" — chỉ Active series mới được submit
            if (series.Status != SeriesStatusActive)
                throw new InvalidOperationException(
                    $"Series phải ở trạng thái Active để submit manuscript. Trạng thái hiện tại: {series.Status}");

            // 5. Check không có manuscript nào đang Approved (BR-80 — locked)
            var hasApproved = await _manuscriptRepository.HasApprovedManuscriptAsync(chapterId, ct);
            if (hasApproved)
                throw new InvalidOperationException(
                    "Chapter này đã có manuscript được Approved. Không thể submit thêm (BR-80).");

            // 6. Check tất cả PageTask của chapter đều ở trạng thái Approved (BR-67)
            var totalTasks = await _context.PageTasks
                .Where(t => t.ChapterId == chapterId)
                .CountAsync(ct);

            if (totalTasks == 0)
                throw new InvalidOperationException(
                    "Chapter chưa có PageTask nào. Phải có ít nhất 1 PageTask trước khi submit (BR-67).");

            var unapprovedCount = await _context.PageTasks
                .Where(t => t.ChapterId == chapterId && t.Status != PageTaskStatusApproved)
                .CountAsync(ct);

            if (unapprovedCount > 0)
                throw new InvalidOperationException(
                    $"Còn {unapprovedCount} PageTask chưa Approved. Tất cả task phải Approved trước khi submit (BR-67).");

            // 7. Tính VersionNo tiếp theo (BR-73)
            var nextVersionNo = await _manuscriptRepository.GetNextVersionNoAsync(chapterId, ct);

            // 8. Tạo Manuscript mới
            var manuscript = new Manuscript
            {
                ManuscriptId = Guid.NewGuid(),
                ChapterId = chapterId,
                VersionNo = nextVersionNo,
                Status = StatusSubmitted,
                SubmittedBy = currentUserId,
                SubmittedAt = DateTime.UtcNow,
                ReviewedBy = null,
                ReviewedAt = null,
                ApprovedAt = null,
                // RevisionCount: version 1 = round 0, version 2 = đã qua 1 revision, v.v.
                RevisionCount = nextVersionNo - 1,
                Feedback = null,
                PreviewFileAssetId = request.PreviewFileAssetId,
                SourceFileAssetId = request.SourceFileAssetId
            };

            await _manuscriptRepository.AddAsync(manuscript, ct);

            // 9. Cập nhật Chapter.Status = "Submitted" (chapter level lifecycle)
            chapter.Status = ChapterStatusSubmitted;

            await _manuscriptRepository.SaveChangeAsync(ct);

            // TODO: Ghi audit log CREATE (BR-128, BR-129)
            // AuditLogService.Log(ActorId=currentUserId, Action="CREATE", EntityType="Manuscript",
            //     EntityId=manuscript.ManuscriptId, NewValue=SerializeManuscript(manuscript));

            return MapToResponse(manuscript);
        }

        /// <inheritdoc />
        public async Task<List<ManuscriptSummaryResponse>> GetListByChapterAsync(
            Guid chapterId,
            Guid currentUserId,
            CancellationToken ct = default)
        {
            // 1. Load Chapter + Series — 404 nếu không tìm thấy
            var chapter = await _context.Chapters
                .Include(c => c.Series)
                .FirstOrDefaultAsync(c => c.ChapterId == chapterId, ct);

            if (chapter == null)
                throw new KeyNotFoundException($"Không tìm thấy chapter với ID: {chapterId}");

            var series = chapter.Series;

            // 2. Check quyền: phải là Mangaka owner, Tantou Editor assigned, hoặc Admin (BR-74)
            if (!_currentUserService.BypassAuthorization)
            {
                var userRoles = await GetUserRolesAsync(currentUserId, ct);
                var canView =
                    (userRoles.Contains(RoleMangaka) && series.MangakaId == currentUserId) ||
                    (userRoles.Contains(RoleTantouEditor) && series.TantouEditorId == currentUserId) ||
                    userRoles.Contains(RoleAdmin);

                if (!canView)
                    throw new UnauthorizedAccessException(
                        "Bạn không có quyền xem manuscript của series này.");
            }

            // 3. Lấy danh sách manuscripts, sort theo VersionNo
            var manuscripts = await _manuscriptRepository.GetByChapterIdAsync(chapterId, ct);

            return manuscripts.Select(MapToSummary).ToList();
        }

        /// <inheritdoc />
        public async Task<ManuscriptResponse> GetByIdAsync(
            Guid manuscriptId,
            Guid currentUserId,
            CancellationToken ct = default)
        {
            // 1. Load Manuscript với Chapter + Series
            var manuscript = await _manuscriptRepository.GetByIdWithDetailsAsync(manuscriptId, ct);
            if (manuscript == null)
                throw new KeyNotFoundException($"Không tìm thấy manuscript với ID: {manuscriptId}");

            var series = manuscript.Chapter.Series;

            // 2. Check quyền (BR-74)
            if (!_currentUserService.BypassAuthorization)
            {
                var userRoles = await GetUserRolesAsync(currentUserId, ct);
                var canView =
                    (userRoles.Contains(RoleMangaka) && series.MangakaId == currentUserId) ||
                    (userRoles.Contains(RoleTantouEditor) && series.TantouEditorId == currentUserId) ||
                    userRoles.Contains(RoleAdmin);

                if (!canView)
                    throw new UnauthorizedAccessException(
                        "Bạn không có quyền xem manuscript này.");
            }

            return MapToResponse(manuscript);
        }

        /// <inheritdoc />
        public async Task<ManuscriptResponse> StartReviewAsync(
            Guid manuscriptId,
            Guid currentUserId,
            CancellationToken ct = default)
        {
            // 1. Kiểm tra role: phải là Tantou Editor (BR-74)
            if (!_currentUserService.BypassAuthorization)
            {
                var userRoles = await GetUserRolesAsync(currentUserId, ct);
                if (!userRoles.Contains(RoleTantouEditor))
                    throw new UnauthorizedAccessException("Chỉ Tantou Editor mới được bắt đầu review.");
            }

            // 2. Load Manuscript với Chapter + Series
            var manuscript = await _manuscriptRepository.GetByIdWithDetailsAsync(manuscriptId, ct);
            if (manuscript == null)
                throw new KeyNotFoundException($"Không tìm thấy manuscript với ID: {manuscriptId}");

            var series = manuscript.Chapter.Series;

            // 3. Check object-level: editor phải phụ trách series này (BR-74)
            if (!_currentUserService.BypassAuthorization && series.TantouEditorId != currentUserId)
                throw new UnauthorizedAccessException("Bạn không phụ trách series này.");

            // 4. Check manuscript phải là latest version (BR-75)
            var latestManuscript = await _manuscriptRepository.GetLatestByChapterIdAsync(
                manuscript.ChapterId, ct);

            if (latestManuscript == null || manuscript.ManuscriptId != latestManuscript.ManuscriptId)
                throw new InvalidOperationException(
                    $"Chỉ được review latest version (v{latestManuscript?.VersionNo}). " +
                    $"Manuscript này là v{manuscript.VersionNo} (BR-75).");

            // 5. Check status phải là "Submitted" (BR-76 — không skip step)
            if (manuscript.Status != StatusSubmitted)
                throw new InvalidOperationException(
                    $"Manuscript phải ở trạng thái Submitted để bắt đầu review. " +
                    $"Trạng thái hiện tại: {manuscript.Status} (BR-76).");

            // 6. Cập nhật status
            manuscript.Status = StatusUnderReview;
            manuscript.ReviewedBy = currentUserId;
            manuscript.ReviewedAt = DateTime.UtcNow;

            _manuscriptRepository.Update(manuscript);
            await _manuscriptRepository.SaveChangeAsync(ct);

            // TODO: Ghi audit log (BR-128)
            // AuditLogService.Log(ActorId=currentUserId, Action="UPDATE", EntityType="Manuscript",
            //     EntityId=manuscriptId, OldValue="Submitted", NewValue="Under Review");

            return MapToResponse(manuscript);
        }

        /// <inheritdoc />
        public async Task<ManuscriptResponse> ApproveAsync(
            Guid manuscriptId,
            Guid currentUserId,
            CancellationToken ct = default)
        {
            // 1. Kiểm tra role: phải là Tantou Editor (BR-74)
            if (!_currentUserService.BypassAuthorization)
            {
                var userRoles = await GetUserRolesAsync(currentUserId, ct);
                if (!userRoles.Contains(RoleTantouEditor))
                    throw new UnauthorizedAccessException("Chỉ Tantou Editor mới được Approve manuscript.");
            }

            // 2. Load Manuscript với Chapter + Series
            var manuscript = await _manuscriptRepository.GetByIdWithDetailsAsync(manuscriptId, ct);
            if (manuscript == null)
                throw new KeyNotFoundException($"Không tìm thấy manuscript với ID: {manuscriptId}");

            var chapter = manuscript.Chapter;
            var series = chapter.Series;

            // 3. Check object-level (BR-74)
            if (!_currentUserService.BypassAuthorization && series.TantouEditorId != currentUserId)
                throw new UnauthorizedAccessException("Bạn không phụ trách series này.");

            // 4. Check manuscript phải là latest version (BR-75)
            var latestManuscript = await _manuscriptRepository.GetLatestByChapterIdAsync(
                chapter.ChapterId, ct);

            if (latestManuscript == null || manuscript.ManuscriptId != latestManuscript.ManuscriptId)
                throw new InvalidOperationException(
                    $"Chỉ được Approve latest version (v{latestManuscript?.VersionNo}). " +
                    $"Manuscript này là v{manuscript.VersionNo} (BR-75).");

            // 5. Check status phải là "Under Review"
            if (manuscript.Status != StatusUnderReview)
                throw new InvalidOperationException(
                    $"Manuscript phải ở trạng thái Under Review để Approve. " +
                    $"Trạng thái hiện tại: {manuscript.Status}.");

            // 6. BR-84: Check chapter completion = 100%
            var totalTasks = await _context.PageTasks
                .Where(t => t.ChapterId == chapter.ChapterId)
                .CountAsync(ct);

            var approvedTasks = await _context.PageTasks
                .Where(t => t.ChapterId == chapter.ChapterId && t.Status == PageTaskStatusApproved)
                .CountAsync(ct);

            if (totalTasks == 0 || approvedTasks < totalTasks)
                throw new InvalidOperationException(
                    $"Chapter completion chưa đạt 100%. " +
                    $"Approved: {approvedTasks}/{totalTasks} tasks (BR-84).");

            // 7. BR-80: Update Manuscript status → Approved, lock
            manuscript.Status = StatusApproved;
            manuscript.ApprovedAt = DateTime.UtcNow;

            // Ghi lại reviewedBy nếu chưa có (edge case: editor approve ngay từ Submitted nếu StartReview bị bỏ qua)
            if (manuscript.ReviewedBy == null)
            {
                manuscript.ReviewedBy = currentUserId;
                manuscript.ReviewedAt = DateTime.UtcNow;
            }

            _manuscriptRepository.Update(manuscript);

            // 8. Publish Chapter
            chapter.Status = ChapterStatusPublished;

            await _manuscriptRepository.SaveChangeAsync(ct);

            // TODO: Ghi audit log cho Manuscript Approved (BR-128)
            // AuditLogService.Log(ActorId=currentUserId, Action="UPDATE", EntityType="Manuscript",
            //     EntityId=manuscriptId, OldValue="Under Review", NewValue="Approved");

            // TODO: Ghi audit log cho Chapter Published
            // AuditLogService.Log(ActorId=currentUserId, Action="UPDATE", EntityType="Chapter",
            //     EntityId=chapter.ChapterId, OldValue="Submitted", NewValue="Published");

            // TODO: Gửi notification tới Mangaka (BR-38)
            // NotificationService.Send(toUserId=series.MangakaId, type="ManuscriptApproved",
            //     payload={ chapterId=chapter.ChapterId, manuscriptId=manuscriptId });

            return MapToResponse(manuscript);
        }

        /// <inheritdoc />
        public async Task<ManuscriptResponse> RequestRevisionAsync(
            Guid manuscriptId,
            Guid currentUserId,
            RequestRevisionRequest request,
            CancellationToken ct = default)
        {
            // 1. Kiểm tra role: phải là Tantou Editor (BR-74)
            if (!_currentUserService.BypassAuthorization)
            {
                var userRoles = await GetUserRolesAsync(currentUserId, ct);
                if (!userRoles.Contains(RoleTantouEditor))
                    throw new UnauthorizedAccessException(
                        "Chỉ Tantou Editor mới được Request Revision.");
            }

            // 2. Load Manuscript với Chapter + Series
            var manuscript = await _manuscriptRepository.GetByIdWithDetailsAsync(manuscriptId, ct);
            if (manuscript == null)
                throw new KeyNotFoundException($"Không tìm thấy manuscript với ID: {manuscriptId}");

            var chapter = manuscript.Chapter;
            var series = chapter.Series;

            // 3. Check object-level (BR-74)
            if (!_currentUserService.BypassAuthorization && series.TantouEditorId != currentUserId)
                throw new UnauthorizedAccessException("Bạn không phụ trách series này.");

            // 4. Check manuscript phải là latest version (BR-75)
            var latestManuscript = await _manuscriptRepository.GetLatestByChapterIdAsync(
                chapter.ChapterId, ct);

            if (latestManuscript == null || manuscript.ManuscriptId != latestManuscript.ManuscriptId)
                throw new InvalidOperationException(
                    $"Chỉ được Request Revision trên latest version (v{latestManuscript?.VersionNo}). " +
                    $"Manuscript này là v{manuscript.VersionNo} (BR-75).");

            // 5. Check status phải là "Under Review"
            if (manuscript.Status != StatusUnderReview)
                throw new InvalidOperationException(
                    $"Manuscript phải ở trạng thái Under Review để Request Revision. " +
                    $"Trạng thái hiện tại: {manuscript.Status}.");

            // 6. BR-77: Check Feedback không rỗng (validation attribute đã check, service check thêm)
            var trimmedFeedback = request.Feedback?.Trim();
            if (string.IsNullOrEmpty(trimmedFeedback))
                throw new ArgumentException("Feedback không được rỗng khi Request Revision (BR-77).");

            // 7. BR-77: Check có ít nhất 1 Annotation trên version này
            var annotationCount = await _annotationRepository.CountByManuscriptVersionAsync(
                manuscriptId, manuscript.VersionNo, ct);

            if (annotationCount == 0)
                throw new InvalidOperationException(
                    "Phải có ít nhất 1 annotation trên manuscript trước khi Request Revision (BR-77). " +
                    "Vui lòng thêm annotation để chỉ ra cụ thể cần sửa ở đâu.");

            // 8. BR-83: Check số revision rounds — max 3 rounds
            if (manuscript.RevisionCount >= MaxRevisionRounds)
            {
                // TODO: Tạo Escalation record — Khiêm xử lý phần này
                // EscalationService.CreateAsync(chapterId=chapter.ChapterId, manuscriptId=manuscriptId,
                //     reason="Exceeded max revision rounds (3)", triggeredBy=currentUserId);

                throw new InvalidOperationException(
                    $"Đã đạt tối đa {MaxRevisionRounds} revision rounds. Cần escalation lên Editorial Board (BR-83).");
            }

            // 9. Cập nhật status và feedback
            manuscript.Status = StatusRevisionRequired;
            manuscript.Feedback = trimmedFeedback;

            _manuscriptRepository.Update(manuscript);

            // 10. Cập nhật Chapter.Status = "Revision Required" (chapter level)
            chapter.Status = ChapterStatusRevisionRequired;

            await _manuscriptRepository.SaveChangeAsync(ct);

            // TODO: Ghi audit log (BR-128)
            // AuditLogService.Log(ActorId=currentUserId, Action="UPDATE", EntityType="Manuscript",
            //     EntityId=manuscriptId, OldValue="Under Review", NewValue="Revision Required");

            // TODO: Gửi notification tới Mangaka
            // NotificationService.Send(toUserId=series.MangakaId, type="RevisionRequested",
            //     payload={ manuscriptId=manuscriptId, feedback=trimmedFeedback });

            return MapToResponse(manuscript);
        }

        // ─── Private helpers ────────────────────────────────────────────────────────

        /// <summary>
        /// Lấy danh sách role names của user (dạng string để so sánh với constants).
        /// Copy pattern từ AnnotationService.
        /// </summary>
        private async Task<HashSet<string>> GetUserRolesAsync(
            Guid userId,
            CancellationToken ct)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync(ct);

            return new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Map Manuscript entity sang ManuscriptResponse DTO.</summary>
        private static ManuscriptResponse MapToResponse(Manuscript manuscript)
        {
            return new ManuscriptResponse
            {
                ManuscriptId = manuscript.ManuscriptId,
                ChapterId = manuscript.ChapterId,
                VersionNo = manuscript.VersionNo,
                Status = manuscript.Status,
                Feedback = manuscript.Feedback,
                SubmittedBy = manuscript.SubmittedBy,
                SubmittedAt = manuscript.SubmittedAt,
                ReviewedBy = manuscript.ReviewedBy,
                ReviewedAt = manuscript.ReviewedAt,
                ApprovedAt = manuscript.ApprovedAt,
                RevisionCount = manuscript.RevisionCount,
                PreviewFileAssetId = manuscript.PreviewFileAssetId,
                SourceFileAssetId = manuscript.SourceFileAssetId
            };
        }

        /// <summary>Map Manuscript entity sang ManuscriptSummaryResponse DTO (nhẹ hơn, cho list).</summary>
        private static ManuscriptSummaryResponse MapToSummary(Manuscript manuscript)
        {
            return new ManuscriptSummaryResponse
            {
                ManuscriptId = manuscript.ManuscriptId,
                VersionNo = manuscript.VersionNo,
                Status = manuscript.Status,
                SubmittedBy = manuscript.SubmittedBy,
                SubmittedAt = manuscript.SubmittedAt,
                RevisionCount = manuscript.RevisionCount
            };
        }
    }
}
