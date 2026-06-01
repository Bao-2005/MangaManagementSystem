using System;

namespace MangaManagementSystem.Business.Manuscripts.DTOs
{
    /// <summary>
    /// Response nhẹ cho danh sách Manuscript (history versions).
    /// Chỉ chứa các field cần thiết để hiển thị list — không trả về toàn bộ detail.
    /// </summary>
    public class ManuscriptSummaryResponse
    {
        /// <summary>ID của manuscript.</summary>
        public Guid ManuscriptId { get; set; }

        /// <summary>Số version (v1, v2, v3, ...).</summary>
        public int VersionNo { get; set; }

        /// <summary>Trạng thái hiện tại.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>UserId của người đã submit.</summary>
        public Guid SubmittedBy { get; set; }

        /// <summary>Thời điểm submit.</summary>
        public DateTime? SubmittedAt { get; set; }

        /// <summary>Số lần revision đã thực hiện.</summary>
        public int RevisionCount { get; set; }
    }
}
