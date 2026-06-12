using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;

namespace MangaManagementSystem.Business.Services.Interfaces.Series
{
    public interface ISeriesProposalWorkflowService
    {
        Task<SeriesDetailResponse> SubmitForReviewAsync(Guid seriesId, Guid mangakaId);
        Task<SeriesDetailResponse> RejectAsync(Guid seriesId, Guid tantouEditorId, RejectProposalRequest request);
        Task<BoardDecisionResponse> SubmitToBoardAsync(Guid seriesId, Guid tantouEditorId);
        Task<SeriesDetailResponse> ActivateAsync(Guid seriesId, Guid tantouEditorId);
    }
}
