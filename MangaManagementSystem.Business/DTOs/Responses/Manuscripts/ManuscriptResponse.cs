namespace MangaManagementSystem.Business.DTOs.Responses.Manuscripts
{
    public class ManuscriptResponse
    {
        public Guid ManuscriptId { get; set; }
        public Guid ChapterId { get; set; }
        public int VersionNo { get; set; }
        public string FileUrl { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Feedback { get; set; }
        public int RevisionCount { get; set; }
        public DateTime SubmittedAt { get; set; }
        public Guid? ReviewedBy { get; set; }
        public string? ReviewerName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
