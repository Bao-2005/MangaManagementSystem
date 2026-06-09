using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Chapters
{
    public class CreateChapterRequest
    {
        [Required]
        public Guid SeriesId { get; set; }

        [Required]
        public int ChapterNo { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = null!;

        [Required]
        public int TotalPages { get; set; }

        public DateTime? PublicationDate { get; set; }

        public DateTime? SubmissionDeadline { get; set; }
    }
}
