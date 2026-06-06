namespace MangaManagementSystem.Business.DTOs.Responses
{
    public class RankingSnapshotResponse
    {
        public Guid RankingSnapshotId { get; set; }
        public Guid SeriesId { get; set; }
        public string SeriesTitle { get; set; } = null!;
        public string Period { get; set; } = null!;
        public int RankNo { get; set; }
        public bool IsBottom20Percent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
