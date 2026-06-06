namespace MangaManagementSystem.Business.DTOs.Responses
{
    public class SeriesDetailResponse : SeriesResponse
    {
        public List<ProposalPageResponse> ProposalPages { get; set; } = new();
    }
}
