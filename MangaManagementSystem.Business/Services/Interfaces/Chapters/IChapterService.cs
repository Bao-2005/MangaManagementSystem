using MangaManagementSystem.Business.DTOs.Requests.Chapters;
using MangaManagementSystem.Business.DTOs.Responses.Chapters;

namespace MangaManagementSystem.Business.Services.Interfaces.Chapters
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
