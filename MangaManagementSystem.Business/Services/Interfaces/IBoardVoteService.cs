using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IBoardVoteService
    {
        Task<IEnumerable<BoardVoteResponse>> GetByDecisionAsync(Guid boardDecisionId);
        Task<BoardVoteResponse> CastVoteAsync(Guid voterId, CreateBoardVoteRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
