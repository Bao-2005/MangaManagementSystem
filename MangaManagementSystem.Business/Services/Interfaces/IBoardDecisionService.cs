using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IBoardDecisionService
    {
        Task<IEnumerable<BoardDecisionResponse>> GetBySeriesAsync(Guid seriesId);
        Task<BoardDecisionResponse> GetByIdAsync(Guid id);
        Task<BoardDecisionResponse> CreateAsync(CreateBoardDecisionRequest request);
        Task<BoardDecisionResponse> UpdateAsync(Guid id, UpdateBoardDecisionRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
