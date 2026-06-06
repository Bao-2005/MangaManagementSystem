using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IProposalPageService
    {
        Task<IEnumerable<ProposalPageResponse>> GetBySeriesAsync(Guid seriesId);
        Task<ProposalPageResponse> GetByIdAsync(Guid id);
        Task<ProposalPageResponse> CreateAsync(Guid seriesId, CreateProposalPageRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
