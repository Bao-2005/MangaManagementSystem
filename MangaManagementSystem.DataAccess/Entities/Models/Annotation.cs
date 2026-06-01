using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class Annotation
    {
        public Guid AnnotationId { get; set; }

        public Guid ManuscriptId { get; set; }

        /// <summary>
        /// Copy từ Manuscript.VersionNo tại thời điểm tạo annotation (BR-78).
        /// Annotation không auto-migrate sang version mới.
        /// </summary>
        public int VersionNo { get; set; }

        /// <summary>
        /// 1-indexed. Phải nằm trong 1..Chapter.TotalPages.
        /// </summary>
        public int PageNo { get; set; }

        public Guid AuthorId { get; set; }

        /// <summary>
        /// Tọa độ X theo percentage 0–100 (BR-78).
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal PositionX { get; set; }

        /// <summary>
        /// Tọa độ Y theo percentage 0–100 (BR-78).
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal PositionY { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete — không hard delete để giữ audit history (BR-08, BR-128/129).
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Nullable — ChapterPage chỉ giữ nếu project có bảng ChapterPage.
        /// Pin Annotation dùng ManuscriptId + VersionNo + PageNo là chính.
        /// </summary>
        public Guid? ChapterPageId { get; set; }

        // Navigation properties
        public Manuscript Manuscript { get; set; } = null!;

        public User Author { get; set; } = null!;

        public ChapterPage? ChapterPage { get; set; }
    }
}
