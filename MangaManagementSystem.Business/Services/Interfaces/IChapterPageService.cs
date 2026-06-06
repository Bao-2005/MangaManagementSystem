using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IChapterPageService
    {
        Task<IEnumerable<ChapterPageResponse>> GetByChapterAsync(Guid chapterId);
        Task<ChapterPageResponse> GetByIdAsync(Guid id);
        Task<ChapterPageResponse> CreateAsync(CreateChapterPageRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
