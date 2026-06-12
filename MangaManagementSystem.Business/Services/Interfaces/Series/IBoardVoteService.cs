using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;

namespace MangaManagementSystem.Business.Services.Interfaces.Series
{
    public interface IBoardVoteService
    {
        Task<IEnumerable<BoardVoteResponse>> GetByDecisionAsync(Guid boardDecisionId);
        Task<BoardDecisionSummaryResponse> CastVoteAsync(Guid voterId, Guid boardDecisionId, CreateBoardVoteRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
