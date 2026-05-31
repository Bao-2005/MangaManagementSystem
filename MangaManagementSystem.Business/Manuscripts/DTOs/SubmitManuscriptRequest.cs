using System;

namespace MangaManagementSystem.Business.Manuscripts.DTOs
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
    }
}
