using System;
using System.Text.Json.Serialization;

namespace MangaManagementSystem.Business.DTOs.Responses
{
    /// <summary>
    /// Response đầy đủ cho một Manuscript — dùng cho các API trả về detail.
    /// </summary>
    public class ManuscriptResponse
    {
        /// <summary>ID của manuscript (PK).</summary>
        [JsonPropertyName("id")]
        public Guid ManuscriptId { get; set; }

        /// <summary>ID của chapter mà manuscript này thuộc về.</summary>
        public Guid ChapterId { get; set; }

        /// <summary>Số version (int, dùng nội bộ).</summary>
        public int VersionNo { get; set; }

        /// <summary>Version theo format "v1", "v2", "v3", ... (dùng cho FE hiển thị).</summary>
        [JsonPropertyName("latestVersion")]
        public string LatestVersion => $"v{VersionNo}";

        /// <summary>Trạng thái hiện tại của manuscript.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Feedback của Editor khi Request Revision (BR-77). Null nếu chưa có.</summary>
        [JsonPropertyName("revisionNotes")]
        public string? Feedback { get; set; }

        /// <summary>Ghi chú của Mangaka gửi kèm khi submit bản thảo.</summary>
        public string? Notes { get; set; }

        /// <summary>UserId của người đã submit (BR-72).</summary>
        public Guid SubmittedBy { get; set; }

        /// <summary>Thời điểm submit.</summary>
        public DateTime? SubmittedAt { get; set; }

        /// <summary>UserId của Editor đã review. Null nếu chưa review.</summary>
        public Guid? ReviewedBy { get; set; }

        /// <summary>Thời điểm Editor bắt đầu review.</summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>Thời điểm được Approve. Null nếu chưa approved.</summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>Số lần revision đã thực hiện (BR-83, tối đa 3 rounds).</summary>
        public int RevisionCount { get; set; }

        /// <summary>ID của file preview (thumbnail). Null nếu không có.</summary>
        public Guid? PreviewFileAssetId { get; set; }

        /// <summary>ID của file gốc (source file). Null nếu không có.</summary>
        public Guid? SourceFileAssetId { get; set; }

        // ─── Fields bổ sung theo API Contract ────────────────────────────────────

        /// <summary>ID của series mà manuscript này thuộc về.</summary>
        public Guid? SeriesId { get; set; }

        /// <summary>Tên series.</summary>
        public string? SeriesTitle { get; set; }

        /// <summary>Số thứ tự chương.</summary>
        public int ChapterNumber { get; set; }

        /// <summary>Tiêu đề chương.</summary>
        public string? ChapterTitle { get; set; }

        /// <summary>Tiến độ hoàn thành chapter (% PageTask Approved / Tổng PageTask, 0-100).</summary>
        public int Progress { get; set; }
    }
}
