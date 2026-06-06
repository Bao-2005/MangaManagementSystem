using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IPageTaskService
    {
        Task<IEnumerable<PageTaskResponse>> GetByChapterAsync(Guid chapterId);
        Task<IEnumerable<PageTaskResponse>> GetByManuscriptAsync(Guid manuscriptId);
        Task<IEnumerable<PageTaskResponse>> GetByAssistantAsync(Guid assistantId);
        Task<PageTaskResponse> GetByIdAsync(Guid id);
        Task<PageTaskResponse> CreateAsync(CreatePageTaskRequest request);
        Task<PageTaskResponse> UpdateAsync(Guid id, UpdatePageTaskRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
