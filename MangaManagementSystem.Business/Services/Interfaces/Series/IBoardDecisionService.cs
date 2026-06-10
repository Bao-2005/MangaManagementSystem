using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;

namespace MangaManagementSystem.Business.Services.Interfaces.Series
{
    public interface IBoardDecisionService
    {
        Task<IEnumerable<BoardDecisionResponse>> GetBySeriesAsync(Guid seriesId);
        Task<BoardDecisionResponse> GetByIdAsync(Guid id);
        Task<BoardDecisionResponse> CreateAsync(CreateBoardDecisionRequest request, Guid? createdBy = null);
        Task<BoardDecisionResponse> UpdateAsync(Guid id, UpdateBoardDecisionRequest request);
        Task<BoardDecisionResponse> ExtendDeadlineAsync(Guid id, Guid editorInChiefId, ExtendBoardDecisionRequest request);
        Task<BoardDecisionResponse> SpecialDecisionAsync(Guid id, Guid editorInChiefId, SpecialBoardDecisionRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
