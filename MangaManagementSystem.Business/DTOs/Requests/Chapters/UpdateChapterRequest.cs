using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Chapters
{
    public class UpdateChapterRequest
    {
        [MaxLength(150)]
        public string? Title { get; set; }

        public int? TotalPages { get; set; }

        public DateTime? PublicationDate { get; set; }

        public DateTime? SubmissionDeadline { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }
    }
}
