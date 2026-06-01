using System;
using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    /// <summary>
    /// Request để submit manuscript mới hoặc resubmit (tạo version mới).
    /// File metadata optional — không upload thật, chỉ lưu metadata (BR-79).
    /// </summary>
    public class SubmitManuscriptRequest
    {
        /// <summary>
        /// ID của FileAsset preview (thumbnail). Nullable — để null khi test không có file.
        /// </summary>
        public Guid? PreviewFileAssetId { get; set; }

        /// <summary>
        /// ID của FileAsset gốc (source file). Nullable — để null khi test không có file.
        /// </summary>
        public Guid? SourceFileAssetId { get; set; }

        /// <summary>
        /// Ghi chú của Mangaka gửi kèm cho Biên tập viên khi nộp bản thảo.
        /// Tối đa 2000 ký tự. Nullable.
        /// </summary>
        [MaxLength(2000, ErrorMessage = "Notes tối đa 2000 ký tự")]
        public string? Notes { get; set; }
    }
}
