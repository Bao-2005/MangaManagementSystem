namespace MangaManagementSystem.Business.DTOs.Responses.Series
{
    public class SeriesDetailResponse : SeriesResponse
    {
        public List<ProposalPageResponse> ProposalPages { get; set; } = new();
    }
}
