using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IChapterService
    {
        Task<IEnumerable<ChapterResponse>> GetAllAsync();
        Task<IEnumerable<ChapterResponse>> GetBySeriesAsync(Guid seriesId);
        Task<ChapterResponse> GetByIdAsync(Guid id);
        Task<ChapterResponse> CreateAsync(CreateChapterRequest request);
        Task<ChapterResponse> UpdateAsync(Guid id, UpdateChapterRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
