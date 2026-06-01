using System;
using System.Text.Json.Serialization;

namespace MangaManagementSystem.Business.DTOs.Responses
{
    /// <summary>
    /// Response nhẹ cho danh sách Manuscript (history versions).
    /// Chỉ chứa các field cần thiết để hiển thị list — không trả về toàn bộ detail.
    /// </summary>
    public class ManuscriptSummaryResponse
    {
        /// <summary>ID của manuscript.</summary>
        [JsonPropertyName("id")]
        public Guid ManuscriptId { get; set; }

        /// <summary>Số version (int, dùng nội bộ).</summary>
        public int VersionNo { get; set; }

        /// <summary>Version theo format "v1", "v2", ... (dùng cho FE hiển thị).</summary>
        [JsonPropertyName("versionLabel")]
        public string LatestVersion => $"v{VersionNo}";

        /// <summary>Trạng thái hiện tại.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>UserId của người đã submit.</summary>
        public Guid SubmittedBy { get; set; }

        /// <summary>Thời điểm submit.</summary>
        public DateTime? SubmittedAt { get; set; }

        /// <summary>Số lần revision đã thực hiện.</summary>
        public int RevisionCount { get; set; }

        // ─── Fields bổ sung theo API Contract ────────────────────────────────────

        /// <summary>ID của series.</summary>
        public Guid? SeriesId { get; set; }

        /// <summary>Tên series.</summary>
        public string? SeriesTitle { get; set; }

        /// <summary>Số thứ tự chương.</summary>
        public int ChapterNumber { get; set; }

        /// <summary>Tiêu đề chương.</summary>
        public string? ChapterTitle { get; set; }

        /// <summary>Tiến độ hoàn thành chapter (0-100%).</summary>
        public int Progress { get; set; }
    }
}
