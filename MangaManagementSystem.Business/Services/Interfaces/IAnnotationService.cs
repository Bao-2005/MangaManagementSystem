using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IAnnotationService
    {
        Task<IEnumerable<AnnotationResponse>> GetByManuscriptAsync(Guid manuscriptId);
        Task<AnnotationResponse> GetByIdAsync(Guid id);
        Task<AnnotationResponse> CreateAsync(Guid authorId, CreateAnnotationRequest request);
        Task<AnnotationResponse> UpdateAsync(Guid id, Guid authorId, UpdateAnnotationRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
