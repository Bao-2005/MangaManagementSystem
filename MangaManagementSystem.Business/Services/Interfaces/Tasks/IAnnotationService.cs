using MangaManagementSystem.Business.DTOs.Requests.Tasks;
using MangaManagementSystem.Business.DTOs.Responses.Tasks;

namespace MangaManagementSystem.Business.Services.Interfaces.Tasks
{
    public interface IAnnotationService
    {
        Task<IEnumerable<AnnotationResponse>> GetByManuscriptAsync(Guid manuscriptId, int? pageNo = null);
        Task<AnnotationResponse> GetByManuscriptAnnotationIdAsync(Guid manuscriptId, Guid id, Guid userId);
        Task<IEnumerable<AnnotationResponse>> GetBySubmissionAsync(Guid submissionId, Guid userId, string userRole);
        Task<AnnotationResponse> GetBySubmissionAnnotationIdAsync(Guid submissionId, Guid id, Guid userId, string userRole);
        Task<AnnotationResponse> GetByIdAsync(Guid id);
        Task<AnnotationResponse> CreateAsync(Guid authorId, Guid manuscriptId, CreateAnnotationRequest request);
        Task<AnnotationResponse> CreateForSubmissionAsync(Guid userId, string userRole, Guid submissionId, CreateSubmissionAnnotationRequest request);
        Task<AnnotationResponse> UpdateAsync(Guid id, Guid authorId, UpdateAnnotationRequest request);
        Task<AnnotationResponse> UpdateForManuscriptAsync(Guid manuscriptId, Guid id, Guid userId, UpdateAnnotationRequest request);
        Task<AnnotationResponse> UpdateForSubmissionAsync(Guid submissionId, Guid id, Guid userId, string userRole, UpdateAnnotationRequest request);
        Task SoftDeleteAsync(Guid id);
        Task SoftDeleteAsync(Guid id, Guid authorId);
        Task SoftDeleteForManuscriptAsync(Guid manuscriptId, Guid id, Guid userId);
        Task SoftDeleteForSubmissionAsync(Guid submissionId, Guid id, Guid userId, string userRole);
    }
}
