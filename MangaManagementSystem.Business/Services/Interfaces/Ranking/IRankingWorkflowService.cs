using MangaManagementSystem.Business.DTOs.Requests.Ranking;
using MangaManagementSystem.Business.DTOs.Responses.Ranking;

namespace MangaManagementSystem.Business.Services.Interfaces.Ranking
{
    public interface IRankingWorkflowService
    {
        Task<RankingVoteRecordResponse> CreateVoteRecordAsync(Guid actorId, CreateRankingVoteRecordRequest request);
        Task<RankingRecalculationResponse> ConfirmVoteRecordAsync(Guid actorId, Guid voteRecordId);
        Task<IReadOnlyList<RankingSnapshotDetailResponse>> GetRankingsAsync(string period);
        Task<IReadOnlyList<RankingSnapshotDetailResponse>> GetSeriesRankingHistoryAsync(Guid seriesId);
    }
}
