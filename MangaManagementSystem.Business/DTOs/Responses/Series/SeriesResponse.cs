namespace MangaManagementSystem.Business.DTOs.Responses.Series
{
    public class SeriesResponse
    {
        public Guid SeriesId { get; set; }
        public Guid MangakaId { get; set; }
        public string MangakaName { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Synopsis { get; set; } = null!;
        public string PublicationType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal RankingScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? RejectReason { get; set; }
        public List<string> Genres { get; set; } = new();
    }
}
