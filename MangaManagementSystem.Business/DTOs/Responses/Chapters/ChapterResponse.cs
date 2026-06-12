namespace MangaManagementSystem.Business.DTOs.Responses.Chapters
{
    public class ChapterResponse
    {
        public Guid ChapterId { get; set; }
        public Guid SeriesId { get; set; }
        public int ChapterNo { get; set; }
        public string Title { get; set; } = null!;
        public int TotalPages { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? PublicationDate { get; set; }
        public DateTime? SubmissionDeadline { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
