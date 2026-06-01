using System;

namespace MangaManagementSystem.Business.DTOs.Responses
{
    /// <summary>
    /// Response DTO trả về cho client sau khi CREATE hoặc GET annotation.
    /// Không expose IsDeleted (đã filter ở service).
    /// </summary>
    public class AnnotationResponse
    {
        public Guid AnnotationId { get; set; }

        public Guid ManuscriptId { get; set; }

        /// <summary>
        /// Version manuscript tại thời điểm tạo annotation (BR-78).
        /// </summary>
        public int VersionNo { get; set; }

        /// <summary>
        /// Số trang (1-indexed).
        /// </summary>
        public int PageNo { get; set; }

        /// <summary>
        /// Tọa độ X theo percentage 0–100.
        /// </summary>
        public decimal PositionX { get; set; }

        /// <summary>
        /// Tọa độ Y theo percentage 0–100.
        /// </summary>
        public decimal PositionY { get; set; }

        /// <summary>
        /// Nội dung annotation (tối đa 1000 ký tự).
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// UserId của editor tạo annotation.
        /// </summary>
        public Guid AuthorId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
