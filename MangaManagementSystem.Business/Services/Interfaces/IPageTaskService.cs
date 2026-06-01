using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IPageTaskService
    {
        Task<PageTaskResponse> CreateAsync(Guid mangakaId, CreatePageTaskRequest request);

        Task<IReadOnlyCollection<PageTaskResponse>> GetForMangakaAsync(Guid mangakaId);

        Task<IReadOnlyCollection<PageTaskResponse>> GetForAssistantAsync(Guid assistantId);

        Task<PageTaskSubmissionResponse> SubmitAsync(Guid assistantId, Guid pageTaskId, SubmitPageTaskRequest request);

        Task<PageTaskSubmissionResponse> ApproveSubmissionAsync(Guid mangakaId, Guid submissionId);

        Task<PageTaskSubmissionResponse> RejectSubmissionAsync(Guid mangakaId, Guid submissionId, RejectPageTaskSubmissionRequest request);
    }
}
