using MangaManagementSystem.Business.DTOs.Responses.Series;

namespace MangaManagementSystem.Business.Services.Interfaces.Series
{
    public interface IBoardDecisionFinalizationService
    {
        Task RecalculateAsync(Guid boardDecisionId);
        Task<BoardDecisionSummaryResponse> ProcessDeadlineAsync(Guid boardDecisionId);
        Task<BoardDecisionSummaryResponse> GetSummaryAsync(Guid boardDecisionId);
    }
}
