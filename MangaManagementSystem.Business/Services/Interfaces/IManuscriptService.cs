using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IManuscriptService
    {
        Task<IEnumerable<ManuscriptResponse>> GetByChapterAsync(Guid chapterId);
        Task<ManuscriptResponse> GetByIdAsync(Guid id);
        Task<ManuscriptResponse> CreateAsync(Guid mangakaId, CreateManuscriptRequest request);
        Task<ManuscriptResponse> UpdateAsync(Guid id, UpdateManuscriptRequest request, Guid reviewerId);
        Task SoftDeleteAsync(Guid id);
    }
}
