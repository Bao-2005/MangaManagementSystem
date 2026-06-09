namespace MangaManagementSystem.Business.DTOs.Responses.Series
{
    public class ProposalPageResponse
    {
        public Guid ProposalPageId { get; set; }
        public Guid SeriesId { get; set; }
        public int PageNo { get; set; }
        public Guid PreviewFileAssetId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
