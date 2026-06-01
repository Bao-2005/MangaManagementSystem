using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    /// <summary>
    /// Service xử lý toàn bộ business logic cho Manuscript feature.
    /// Enforce BR-67, BR-72, BR-73, BR-74, BR-75, BR-76, BR-77, BR-80, BR-83, BR-84.
    /// </summary>
    public interface IManuscriptService
    {
        /// <summary>
        /// Mangaka submit manuscript mới hoặc resubmit (tạo version mới).
        /// Enforce BR-67 (tất cả PageTask phải Approved), BR-72 (chỉ Mangaka owner),
        /// BR-73 (versioning), BR-80 (không submit nếu đã Approved).
        /// </summary>
        Task<ManuscriptResponse> SubmitAsync(
            Guid chapterId,
            Guid currentUserId,
            SubmitManuscriptRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy danh sách tất cả manuscript versions của một chapter (history).
        /// Enforce BR-74: Mangaka owner, Tantou Editor assigned, hoặc Admin mới xem được.
        /// </summary>
        Task<List<ManuscriptSummaryResponse>> GetListByChapterAsync(
            Guid chapterId,
            Guid currentUserId,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy toàn bộ manuscripts trong hệ thống (dành cho Tantou Editor / Admin).
        /// Tương ứng endpoint GET /manuscripts theo API Contract.
        /// </summary>
        Task<List<ManuscriptSummaryResponse>> GetAllAsync(
            Guid currentUserId,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy chi tiết một manuscript theo ID.
        /// Enforce BR-74: chỉ những người có quyền với series mới được xem.
        /// </summary>
        Task<ManuscriptResponse> GetByIdAsync(
            Guid manuscriptId,
            Guid currentUserId,
            CancellationToken ct = default);

        /// <summary>
        /// Editor bắt đầu review — chuyển status Submitted → Under Review.
        /// Enforce BR-74 (assigned editor), BR-75 (phải là latest version), BR-76 (đúng flow).
        /// </summary>
        Task<ManuscriptResponse> StartReviewAsync(
            Guid manuscriptId,
            Guid currentUserId,
            CancellationToken ct = default);

        /// <summary>
        /// Editor approve manuscript — chuyển status Under Review → Approved.
        /// Đồng thời publish chapter (Chapter.Status = "Published").
        /// Enforce BR-74, BR-75, BR-80 (lock), BR-84 (completion 100%).
        /// </summary>
        Task<ManuscriptResponse> ApproveAsync(
            Guid manuscriptId,
            Guid currentUserId,
            CancellationToken ct = default);

        /// <summary>
        /// Editor yêu cầu sửa — chuyển status Under Review → Revision Required.
        /// Enforce BR-74, BR-75, BR-77 (cần annotation + feedback), BR-83 (max 3 rounds).
        /// </summary>
        Task<ManuscriptResponse> RequestRevisionAsync(
            Guid manuscriptId,
            Guid currentUserId,
            RequestRevisionRequest request,
            CancellationToken ct = default);
    }
}
