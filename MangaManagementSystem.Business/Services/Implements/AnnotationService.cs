using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.Business.Auth.Interfaces;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.Services.Implements
{
    public class AnnotationService : IAnnotationService
    {
        private readonly IAnnotationRepository _annotationRepository;
        private readonly ICurrentUserService _currentUserService;

        // Role name constants — khớp với giá trị trong bảng Roles
        private const string RoleTantouEditor = "Tantou Editor";
        private const string RoleMangaka = "MANGAKA";
        private const string RoleAdmin = "ADMIN";

        // Manuscript status constants - lấy từ Enum
        private static readonly string ManuscriptStatusApproved = ManuscriptStatus.Approved.ToStorageValue();
        private static readonly string ManuscriptStatusDraft = ManuscriptStatus.Draft.ToStorageValue();

        public AnnotationService(
            IAnnotationRepository annotationRepository,
            ICurrentUserService currentUserService)
        {
            _annotationRepository = annotationRepository;
            _currentUserService = currentUserService;
        }

        /// <inheritdoc />
        public async Task<AnnotationResponse> CreateAsync(
            Guid manuscriptId,
            Guid currentUserId,
            CreateAnnotationRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. Kiểm tra role: phải là Tantou Editor (BR-03, BR-74)
            var userRoles = await GetUserRolesAsync(currentUserId, cancellationToken);
            if (!userRoles.Contains(RoleTantouEditor))
                throw new UnauthorizedAccessException("Ònly Tantou Editor mới được tạo annotation.");

            // 2. Load Manuscript + Chapter + Series (BR-04)
            var manuscript = await LoadManuscriptWithDetailsAsync(manuscriptId, cancellationToken);
            if (manuscript == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy manuscript với ID: {manuscriptId}");
            }

            var series = manuscript.Chapter.Series;

            // 3. Kiểm tra object-level authorization: editor phải phụ trách series này (BR-74)
            if (series.TantouEditorId != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không phụ trách series này.");
            }

            // 4. Kiểm tra manuscript là latest version (BR-75)
            var latestVersionNo = await _annotationRepository.GetLatestManuscriptVersionNoAsync(
                manuscript.ChapterId, cancellationToken);

            if (manuscript.VersionNo != latestVersionNo)
            {
                throw new InvalidOperationException(
                    $"Chỉ được tạo annotation trên latest version (v{latestVersionNo}). " +
                    $"Manuscript này là v{manuscript.VersionNo}.");
            }

            // 5. Kiểm tra manuscript chưa Approved (BR-80)
            if (manuscript.Status == ManuscriptStatusApproved)
            {
                throw new InvalidOperationException("Manuscript đã Approved. Không thể tạo annotation.");
            }

            // 6. Không cho annotate khi draft (workflow chưa cho editor review draft)
            if (manuscript.Status == ManuscriptStatusDraft)
            {
                throw new InvalidOperationException("Manuscript đang ở trạng thái Draft. Chờ Mangaka submit trước.");
            }

            // 7. Kiểm tra PageNo hợp lệ
            var totalPages = manuscript.Chapter.TotalPages;
            if (request.PageNo < 1 || request.PageNo > totalPages)
            {
                throw new ArgumentException(
                    $"PageNo phải nằm trong 1..{totalPages}. Nhận được: {request.PageNo}");
            }

            // 8. Validate content (data annotations đã check, nhưng service check thêm để chắc)
            var trimmedContent = request.Content?.Trim();
            if (string.IsNullOrEmpty(trimmedContent))
            {
                throw new ArgumentException("Content không được rỗng.");
            }

            // 9. Tạo annotation — VersionNo copy từ Manuscript (BR-78)
            var annotation = new Annotation
            {
                AnnotationId = Guid.NewGuid(),
                ManuscriptId = manuscriptId,
                VersionNo = manuscript.VersionNo,       // BR-78: bind với version tại thời điểm tạo
                PageNo = request.PageNo,
                AuthorId = currentUserId,
                PositionX = request.PositionX,
                PositionY = request.PositionY,
                Content = trimmedContent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                IsDeleted = AnnotationStatus.Active.ToIsDeleted()
            };

            await _annotationRepository.AddAsync(annotation, cancellationToken);
            await _annotationRepository.SaveChangeAsync(cancellationToken);

            // 10. TODO: Ghi audit log CREATE (BR-128)
            // AuditLogService.Log(ActorId=currentUserId, Action="CREATE", EntityType="Annotation",
            //     EntityId=annotation.AnnotationId, NewValue=SerializeAnnotation(annotation));

            return MapToResponse(annotation);
        }

        /// <inheritdoc />
        public async Task<List<AnnotationResponse>> GetAsync(
            Guid manuscriptId,
            Guid currentUserId,
            int? versionNo = null,
            int? pageNo = null,
            CancellationToken cancellationToken = default)
        {
            // 1. Load manuscript để kiểm tra tồn tại và authorization
            var manuscript = await LoadManuscriptWithDetailsAsync(manuscriptId, cancellationToken);
            if (manuscript == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy manuscript với ID: {manuscriptId}");
            }

            var series = manuscript.Chapter.Series;

            // 2. Kiểm tra quyền xem: phải là assigned editor hoặc mangaka của series (BR-04)
            var userRoles = await GetUserRolesAsync(currentUserId, cancellationToken);
            var canView =
                (userRoles.Contains(RoleTantouEditor) && series.TantouEditorId == currentUserId) ||
                (userRoles.Contains(RoleMangaka) && series.MangakaId == currentUserId) ||
                userRoles.Contains(RoleAdmin);

            if (!canView)
                throw new UnauthorizedAccessException("Bạn không có quyền xem annotation của series này.");

            // 3. Nếu không truyền versionNo, dùng latest version
            var targetVersionNo = versionNo ?? await _annotationRepository.GetLatestManuscriptVersionNoAsync(
                manuscript.ChapterId, cancellationToken) ?? manuscript.VersionNo;

            // 4. Lấy annotations (không trả IsDeleted = true — BR-08)
            var annotations = await _annotationRepository.GetByManuscriptVersionAsync(
                manuscriptId, targetVersionNo, pageNo, cancellationToken);

            return annotations.Select(MapToResponse).ToList();
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(
            Guid manuscriptId,
            Guid currentUserId,
            int? versionNo = null,
            CancellationToken cancellationToken = default)
        {
            var manuscript = await LoadManuscriptWithDetailsAsync(manuscriptId, cancellationToken);
            if (manuscript == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy manuscript với ID: {manuscriptId}");
            }

            var series = manuscript.Chapter.Series;

            // Kiểm tra quyền: editor hoặc mangaka của series
            var userRoles = await GetUserRolesAsync(currentUserId, cancellationToken);
            var canView =
                (userRoles.Contains(RoleTantouEditor) && series.TantouEditorId == currentUserId) ||
                (userRoles.Contains(RoleMangaka) && series.MangakaId == currentUserId) ||
                userRoles.Contains(RoleAdmin);

            if (!canView)
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập annotation của series này.");

            var targetVersionNo = versionNo ?? await _annotationRepository.GetLatestManuscriptVersionNoAsync(
                manuscript.ChapterId, cancellationToken) ?? manuscript.VersionNo;

            return await _annotationRepository.CountByManuscriptVersionAsync(
                manuscriptId, targetVersionNo, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<AnnotationResponse> UpdateAsync(
            Guid manuscriptId,
            Guid annotationId,
            Guid currentUserId,
            UpdateAnnotationRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. Kiểm tra role (BR-03)
            var userRoles = await GetUserRolesAsync(currentUserId, cancellationToken);
            if (!userRoles.Contains(RoleTantouEditor))
                throw new UnauthorizedAccessException("Chỉ Tantou Editor mới được sửa annotation.");

            // 2. Tìm annotation
            var annotation = await _annotationRepository.GetByIdAsync(annotationId, cancellationToken);
            if (annotation == null || annotation.IsDeleted)
            {
                throw new KeyNotFoundException($"Không tìm thấy annotation với ID: {annotationId}");
            }

            // 3. Kiểm tra annotation thuộc manuscript đúng
            if (annotation.ManuscriptId != manuscriptId)
            {
                throw new KeyNotFoundException("Annotation không thuộc manuscript này.");
            }

            // 4. Kiểm tra current user là author của annotation (BR-04)
            if (annotation.AuthorId != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không phải author của annotation này.");
            }

            // 5. Load manuscript + chapter + series
            var manuscript = await LoadManuscriptWithDetailsAsync(manuscriptId, cancellationToken);
            if (manuscript == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy manuscript với ID: {manuscriptId}");
            }

            var series = manuscript.Chapter.Series;

            // 6. Kiểm tra vẫn là assigned editor của series (BR-74)
            if (series.TantouEditorId != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không còn phụ trách series này.");
            }

            // 7. Kiểm tra manuscript chưa Approved (BR-80)
            if (manuscript.Status == ManuscriptStatusApproved)
            {
                throw new InvalidOperationException("Manuscript đã Approved. Không thể sửa annotation.");
            }

            // 8. Kiểm tra annotation thuộc latest version (BR-75)
            var latestVersionNo = await _annotationRepository.GetLatestManuscriptVersionNoAsync(
                manuscript.ChapterId, cancellationToken);

            if (annotation.VersionNo != latestVersionNo)
            {
                throw new InvalidOperationException(
                    $"Chỉ được sửa annotation trên latest version (v{latestVersionNo}). " +
                    $"Annotation này thuộc v{annotation.VersionNo}.");
            }

            // 9. Capture old values để ghi audit log (BR-129)
            // var oldContent = annotation.Content;
            // var oldPositionX = annotation.PositionX;
            // var oldPositionY = annotation.PositionY;

            // 10. Apply updates (chỉ update field được gửi lên)
            bool hasChanges = false;

            if (request.PositionX.HasValue)
            {
                annotation.PositionX = request.PositionX.Value;
                hasChanges = true;
            }

            if (request.PositionY.HasValue)
            {
                annotation.PositionY = request.PositionY.Value;
                hasChanges = true;
            }

            if (request.Content != null)
            {
                var trimmedContent = request.Content.Trim();
                if (string.IsNullOrEmpty(trimmedContent))
                {
                    throw new ArgumentException("Content không được rỗng.");
                }
                annotation.Content = trimmedContent;
                hasChanges = true;
            }

            if (hasChanges)
            {
                annotation.UpdatedAt = DateTime.UtcNow;
                _annotationRepository.Update(annotation);
                await _annotationRepository.SaveChangeAsync(cancellationToken);

                // TODO: Ghi audit log UPDATE (BR-129)
                // AuditLogService.Log(ActorId=currentUserId, Action="UPDATE", EntityType="Annotation",
                //     EntityId=annotationId,
                //     OldValue={ Content=oldContent, PositionX=oldPositionX, PositionY=oldPositionY },
                //     NewValue={ Content=annotation.Content, PositionX=annotation.PositionX, PositionY=annotation.PositionY });
            }

            return MapToResponse(annotation);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(
            Guid manuscriptId,
            Guid annotationId,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            // 1. Kiểm tra role (BR-03)
            var userRoles = await GetUserRolesAsync(currentUserId, cancellationToken);
            if (!userRoles.Contains(RoleTantouEditor))
                throw new UnauthorizedAccessException("Chỉ Tantou Editor mới được xóa annotation.");

            // 2. Tìm annotation
            var annotation = await _annotationRepository.GetByIdAsync(annotationId, cancellationToken);
            if (annotation == null || annotation.IsDeleted)
            {
                throw new KeyNotFoundException($"Không tìm thấy annotation với ID: {annotationId}");
            }

            // 3. Kiểm tra annotation thuộc manuscript đúng
            if (annotation.ManuscriptId != manuscriptId)
            {
                throw new KeyNotFoundException("Annotation không thuộc manuscript này.");
            }

            // 4. Kiểm tra current user là author của annotation (BR-04)
            if (annotation.AuthorId != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không phải author của annotation này.");
            }

            // 5. Load manuscript + chapter + series
            var manuscript = await LoadManuscriptWithDetailsAsync(manuscriptId, cancellationToken);
            if (manuscript == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy manuscript với ID: {manuscriptId}");
            }

            var series = manuscript.Chapter.Series;

            // 6. Kiểm tra vẫn là assigned editor của series (BR-74)
            if (series.TantouEditorId != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không còn phụ trách series này.");
            }

            // 7. Kiểm tra manuscript chưa Approved (BR-80)
            if (manuscript.Status == ManuscriptStatusApproved)
            {
                throw new InvalidOperationException("Manuscript đã Approved. Không thể xóa annotation.");
            }

            // 8. Kiểm tra annotation thuộc latest version (BR-75)
            var latestVersionNo = await _annotationRepository.GetLatestManuscriptVersionNoAsync(
                manuscript.ChapterId, cancellationToken);

            if (annotation.VersionNo != latestVersionNo)
            {
                throw new InvalidOperationException(
                    $"Chỉ được xóa annotation trên latest version (v{latestVersionNo}). " +
                    $"Annotation này thuộc v{annotation.VersionNo}.");
            }

            // 9. Soft delete — không hard delete để giữ audit history (BR-08)
            annotation.IsDeleted = AnnotationStatus.Deleted.ToIsDeleted();
            annotation.UpdatedAt = DateTime.UtcNow;

            _annotationRepository.Update(annotation);
            await _annotationRepository.SaveChangeAsync(cancellationToken);

            // TODO: Ghi audit log DELETE (BR-128)
            // AuditLogService.Log(ActorId=currentUserId, Action="DELETE", EntityType="Annotation",
            //     EntityId=annotationId,
            //     OldValue=SerializeAnnotation(annotation));
        }

        // ─── Private helpers ────────────────────────────────────────────────────────

        /// <summary>
        /// Load manuscript cùng Chapter và Series để check authorization và business rules.
        /// </summary>
        private async Task<Manuscript?> LoadManuscriptWithDetailsAsync(
            Guid manuscriptId,
            CancellationToken cancellationToken)
        {
            return await _annotationRepository.GetManuscriptWithDetailsAsync(manuscriptId, cancellationToken);
        }

        /// <summary>
        /// Lấy danh sách role names của user (dạng string để so sánh với constants).
        /// </summary>
        private async Task<HashSet<string>> GetUserRolesAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var roleName = await _annotationRepository.GetUserRoleNameAsync(userId, cancellationToken);

            return roleName != null
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) { roleName }
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Map Annotation entity sang AnnotationResponse DTO.
        /// </summary>
        private static AnnotationResponse MapToResponse(Annotation annotation)
        {
            return new AnnotationResponse
            {
                AnnotationId = annotation.AnnotationId,
                ManuscriptId = annotation.ManuscriptId,
                VersionNo = annotation.VersionNo,
                PageNo = annotation.PageNo,
                PositionX = annotation.PositionX,
                PositionY = annotation.PositionY,
                Content = annotation.Content,
                AuthorId = annotation.AuthorId,
                CreatedAt = annotation.CreatedAt,
                UpdatedAt = annotation.UpdatedAt
            };
        }
    }
}
