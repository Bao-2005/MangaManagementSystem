using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IVoteRecordService
    {
        Task<IEnumerable<VoteRecordResponse>> GetBySeriesAsync(Guid seriesId);
        Task<IEnumerable<VoteRecordResponse>> GetByPeriodAsync(string period);
        Task<VoteRecordResponse> CreateAsync(CreateVoteRecordRequest request);
        Task<VoteRecordResponse> ConfirmAsync(Guid id, Guid confirmerId);
        Task SoftDeleteAsync(Guid id);
    }
}
