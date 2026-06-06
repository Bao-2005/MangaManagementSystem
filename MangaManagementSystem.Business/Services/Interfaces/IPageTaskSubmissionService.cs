using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IPageTaskSubmissionService
    {
        Task<IEnumerable<SubmissionResponse>> GetByTaskAsync(Guid pageTaskId);
        Task<SubmissionResponse> GetByIdAsync(Guid id);
        Task<SubmissionResponse> CreateAsync(Guid assistantId, CreateSubmissionRequest request);
        Task<SubmissionResponse> UpdateAsync(Guid id, UpdateSubmissionRequest request, Guid reviewerId);
        Task SoftDeleteAsync(Guid id);
    }
}
