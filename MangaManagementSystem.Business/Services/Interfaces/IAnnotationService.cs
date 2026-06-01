using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    /// <summary>
    /// Service xử lý business logic cho Pin Annotation.
    /// Enforce các BR: BR-74, BR-75, BR-77, BR-78, BR-80, BR-128/129.
    /// </summary>
    public interface IAnnotationService
    {
        /// <summary>
        /// Tạo Pin Annotation mới.
        /// Chỉ Tantou Editor được assign cho series mới được tạo (BR-74).
        /// Manuscript phải là latest version và chưa Approved (BR-75, BR-80).
        /// </summary>
        /// <param name="manuscriptId">ID của manuscript cần annotate</param>
        /// <param name="currentUserId">UserId của editor đang đăng nhập</param>
        /// <param name="request">Thông tin pin cần tạo</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Annotation vừa tạo</returns>
        Task<AnnotationResponse> CreateAsync(
            Guid manuscriptId,
            Guid currentUserId,
            CreateAnnotationRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách annotation theo manuscript và version.
        /// Tantou Editor hoặc Mangaka của series mới được xem (BR-04).
        /// Không trả về annotation IsDeleted = true (BR-08).
        /// </summary>
        /// <param name="manuscriptId">ID của manuscript</param>
        /// <param name="currentUserId">UserId đang đăng nhập</param>
        /// <param name="versionNo">Null = latest version</param>
        /// <param name="pageNo">Null = tất cả trang</param>
        /// <param name="cancellationToken"></param>
        Task<List<AnnotationResponse>> GetAsync(
            Guid manuscriptId,
            Guid currentUserId,
            int? versionNo = null,
            int? pageNo = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Đếm annotation theo manuscript version.
        /// Dùng để Manuscript Review module enforce BR-77.
        /// Tantou Editor hoặc Mangaka của series mới được gọi.
        /// </summary>
        Task<int> CountAsync(
            Guid manuscriptId,
            Guid currentUserId,
            int? versionNo = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật position hoặc content của annotation (PATCH).
        /// Chỉ author của annotation mới được sửa (BR-04).
        /// Manuscript phải chưa Approved (BR-80).
        /// Annotation phải thuộc latest version (BR-75).
        /// </summary>
        Task<AnnotationResponse> UpdateAsync(
            Guid manuscriptId,
            Guid annotationId,
            Guid currentUserId,
            UpdateAnnotationRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft delete annotation (IsDeleted = true).
        /// Không hard delete để giữ lịch sử audit (BR-08, BR-128/129).
        /// Chỉ author của annotation mới được xóa (BR-04).
        /// Manuscript phải chưa Approved (BR-80).
        /// Annotation phải thuộc latest version (BR-75).
        /// </summary>
        Task DeleteAsync(
            Guid manuscriptId,
            Guid annotationId,
            Guid currentUserId,
            CancellationToken cancellationToken = default);
    }
}
