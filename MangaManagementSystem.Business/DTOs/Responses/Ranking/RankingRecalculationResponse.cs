namespace MangaManagementSystem.Business.DTOs.Responses.Ranking
{
    public class RankingRecalculationResponse
    {
        public string Period { get; set; } = null!;
        public int TotalRanked { get; set; }
        public IReadOnlyList<RankingSnapshotDetailResponse> Snapshots { get; set; } =
            Array.Empty<RankingSnapshotDetailResponse>();
    }
}
