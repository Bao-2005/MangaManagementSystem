using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;

namespace MangaManagementSystem.Business.Services.Interfaces.Series
{
    public interface IProposalPageService
    {
        Task<IEnumerable<ProposalPageResponse>> GetBySeriesAsync(Guid seriesId);
        Task<ProposalPageResponse> GetByIdAsync(Guid id);
        Task<ProposalPageResponse> CreateAsync(Guid seriesId, CreateProposalPageRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
