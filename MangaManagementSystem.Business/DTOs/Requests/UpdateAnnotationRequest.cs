using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    /// <summary>
    /// Request DTO để cập nhật Pin Annotation (PATCH).
    /// Tất cả fields là optional — chỉ update những field được gửi lên.
    /// </summary>
    public class UpdateAnnotationRequest
    {
        /// <summary>
        /// Tọa độ X mới (percentage 0–100). Null = không thay đổi.
        /// </summary>
        [Range(0, 100, ErrorMessage = "PositionX phải nằm trong 0–100")]
        public decimal? PositionX { get; set; }

        /// <summary>
        /// Tọa độ Y mới (percentage 0–100). Null = không thay đổi.
        /// </summary>
        [Range(0, 100, ErrorMessage = "PositionY phải nằm trong 0–100")]
        public decimal? PositionY { get; set; }

        /// <summary>
        /// Nội dung mới. Null = không thay đổi.
        /// Nếu gửi lên phải không rỗng và tối đa 1000 ký tự.
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Content tối đa 1000 ký tự")]
        public string? Content { get; set; }
    }
}
