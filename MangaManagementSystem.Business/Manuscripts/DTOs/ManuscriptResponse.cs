using System;

namespace MangaManagementSystem.Business.Manuscripts.DTOs
{
    /// <summary>
    /// Response đầy đủ cho một Manuscript — dùng cho các API trả về detail.
    /// </summary>
    public class ManuscriptResponse
    {
        /// <summary>ID của manuscript (PK).</summary>
        public Guid ManuscriptId { get; set; }

        /// <summary>ID của chapter mà manuscript này thuộc về.</summary>
        public Guid ChapterId { get; set; }

        /// <summary>Số version (v1, v2, v3, ...).</summary>
        public int VersionNo { get; set; }

        /// <summary>Trạng thái hiện tại của manuscript.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Feedback của Editor khi Request Revision (BR-77). Null nếu chưa có.</summary>
        public string? Feedback { get; set; }

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
    }
}
