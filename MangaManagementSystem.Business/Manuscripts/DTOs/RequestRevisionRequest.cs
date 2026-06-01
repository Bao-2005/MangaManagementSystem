using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.Manuscripts.DTOs
{
    /// <summary>
    /// Request để Editor yêu cầu Mangaka chỉnh sửa bản thảo (BR-77).
    /// Feedback bắt buộc và phải đủ chi tiết (tối thiểu 10 ký tự).
    /// </summary>
    public class RequestRevisionRequest
    {
        /// <summary>
        /// Nhận xét/phản hồi của Editor giải thích cần sửa gì (BR-77).
        /// Bắt buộc — không được để trống hoặc quá ngắn.
        /// </summary>
        [Required(ErrorMessage = "Feedback là bắt buộc khi yêu cầu revision.")]
        [MinLength(10, ErrorMessage = "Feedback phải có ít nhất 10 ký tự.")]
        [MaxLength(2000, ErrorMessage = "Feedback không được vượt quá 2000 ký tự.")]
        public string Feedback { get; set; } = string.Empty;
    }
}
