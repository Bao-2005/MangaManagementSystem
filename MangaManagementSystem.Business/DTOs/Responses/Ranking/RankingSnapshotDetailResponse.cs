namespace MangaManagementSystem.Business.DTOs.Responses.Ranking
{
    public class RankingSnapshotDetailResponse
    {
        public Guid RankingSnapshotId { get; set; }
        public Guid SeriesId { get; set; }
        public string SeriesTitle { get; set; } = null!;
        public Guid? VoteRecordId { get; set; }
        public string Period { get; set; } = null!;
        public int RankNo { get; set; }
        public decimal Score { get; set; }
        public int ReaderCount { get; set; }
        public int VoteCount { get; set; }
        public bool IsBottom20Percent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
