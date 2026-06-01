using System;
using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    /// <summary>
    /// Request DTO để tạo Pin Annotation mới.
    /// Editor gửi vị trí pin (percentage 0–100) và nội dung comment.
    /// </summary>
    public class CreateAnnotationRequest
    {
        /// <summary>
        /// Số trang (1-indexed). Phải nằm trong 1..Chapter.TotalPages.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PageNo phải >= 1")]
        public int PageNo { get; set; }

        /// <summary>
        /// Tọa độ X theo percentage 0–100.
        /// Ví dụ: 50.00 nghĩa là giữa chiều ngang trang.
        /// </summary>
        [Required]
        [Range(0, 100, ErrorMessage = "PositionX phải nằm trong 0–100")]
        public decimal PositionX { get; set; }

        /// <summary>
        /// Tọa độ Y theo percentage 0–100.
        /// Ví dụ: 25.00 nghĩa là 25% từ trên xuống.
        /// </summary>
        [Required]
        [Range(0, 100, ErrorMessage = "PositionY phải nằm trong 0–100")]
        public decimal PositionY { get; set; }

        /// <summary>
        /// Nội dung annotation. Required, tối đa 1000 ký tự.
        /// </summary>
        [Required(ErrorMessage = "Content là bắt buộc")]
        [MinLength(1, ErrorMessage = "Content không được rỗng")]
        [MaxLength(1000, ErrorMessage = "Content tối đa 1000 ký tự")]
        public string Content { get; set; } = string.Empty;
    }
}
