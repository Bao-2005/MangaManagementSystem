using MangaManagementSystem.Business.DTOs.Requests.Chapters;
using MangaManagementSystem.Business.DTOs.Responses.Chapters;

namespace MangaManagementSystem.Business.Services.Interfaces.Chapters
{
    public interface IChapterPageService
    {
        Task<IEnumerable<ChapterPageResponse>> GetByChapterAsync(Guid chapterId);
        Task<ChapterPageResponse> GetByIdAsync(Guid id);
        Task<ChapterPageResponse> CreateAsync(CreateChapterPageRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
