using MangaManagementSystem.Business.DTOs.Requests.Tasks;
using MangaManagementSystem.Business.DTOs.Responses.Tasks;

namespace MangaManagementSystem.Business.Services.Interfaces.Tasks
{
    public interface IAnnotationService
    {
        Task<IEnumerable<AnnotationResponse>> GetByManuscriptAsync(Guid manuscriptId);
        Task<IEnumerable<AnnotationResponse>> GetBySubmissionAsync(Guid submissionId);
        Task<AnnotationResponse> GetByIdAsync(Guid id);
        Task<AnnotationResponse> CreateAsync(Guid authorId, Guid manuscriptId, CreateAnnotationRequest request);
        Task<AnnotationResponse> CreateForSubmissionAsync(Guid assistantId, Guid submissionId, CreateSubmissionAnnotationRequest request);
        Task<AnnotationResponse> UpdateAsync(Guid id, Guid authorId, UpdateAnnotationRequest request);
        Task<AnnotationResponse> UpdateForSubmissionAsync(Guid submissionId, Guid id, Guid assistantId, UpdateAnnotationRequest request);
        Task SoftDeleteAsync(Guid id);
        Task SoftDeleteAsync(Guid id, Guid authorId);
        Task SoftDeleteForSubmissionAsync(Guid submissionId, Guid id, Guid assistantId);
    }
}
