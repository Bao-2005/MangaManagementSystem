using MangaManagementSystem.Business.DTOs.Requests.Tasks;
using MangaManagementSystem.Business.DTOs.Responses.Tasks;
using MangaManagementSystem.Business.Services.Interfaces.Tasks;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Tasks
{
    public class AnnotationService : IAnnotationService
    {
        private readonly IRepository<Annotation> _repo;
        private readonly IRepository<PageTaskSubmission> _submissionRepo;

        public AnnotationService(IRepository<Annotation> repo, IRepository<PageTaskSubmission> submissionRepo)
        {
            _repo = repo;
            _submissionRepo = submissionRepo;
        }

        public async Task<IEnumerable<AnnotationResponse>> GetByManuscriptAsync(Guid manuscriptId)
            => await _repo.GetAll()
                .Include(a => a.Author)
                .Include(a => a.Manuscript)
                .Where(a => a.ManuscriptId == manuscriptId && a.DeletedAt == null)
                .Select(a => Map(a)).ToListAsync();

        public async Task<IEnumerable<AnnotationResponse>> GetBySubmissionAsync(Guid submissionId)
            => await _repo.GetAll()
                .Include(a => a.Author)
                .Include(a => a.Manuscript)
                .Where(a => a.PageTaskSubmissionId == submissionId && a.DeletedAt == null)
                .Select(a => Map(a)).ToListAsync();

        public async Task<AnnotationResponse> GetByIdAsync(Guid id)
        {
            var a = await _repo.GetAll().Include(a => a.Author).Include(a => a.Manuscript)
                .FirstOrDefaultAsync(x => x.AnnotationId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Annotation not found.");
            return Map(a);
        }

        public async Task<AnnotationResponse> CreateAsync(Guid authorId, CreateAnnotationRequest request)
        {
            ValidateAnnotationPayload(request.PageNo, request.Content);

            var annotation = new Annotation
            {
                ManuscriptId = request.ManuscriptId,
                AuthorId = authorId,
                PageNo = request.PageNo,
                PositionX = request.PositionX,
                PositionY = request.PositionY,
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(annotation);
            await _repo.SaveChangeAsync();
            return await GetByIdAsync(annotation.AnnotationId);
        }

        public async Task<AnnotationResponse> CreateForSubmissionAsync(
            Guid assistantId,
            Guid submissionId,
            CreateSubmissionAnnotationRequest request)
        {
            var submission = await _submissionRepo.GetAll()
                .Include(s => s.PageTask)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId && s.DeletedAt == null)
                ?? throw new KeyNotFoundException("Submission not found.");

            if (submission.PageTask.DeletedAt != null)
                throw new KeyNotFoundException("Page task not found.");

            if (submission.PageTask.AssistantId != assistantId)
                throw new UnauthorizedAccessException("Assistant can only annotate their own submissions.");

            ValidateAnnotationPayload(request.PageNo, request.Content);

            var annotation = new Annotation
            {
                ManuscriptId = submission.PageTask.ManuscriptId,
                PageTaskSubmissionId = submissionId,
                AuthorId = assistantId,
                PageNo = request.PageNo,
                PositionX = request.PositionX,
                PositionY = request.PositionY,
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(annotation);
            await _repo.SaveChangeAsync();
            return await GetByIdAsync(annotation.AnnotationId);
        }

        public async Task<AnnotationResponse> UpdateAsync(Guid id, Guid authorId, UpdateAnnotationRequest request)
        {
            var a = await _repo.GetAll().FirstOrDefaultAsync(x => x.AnnotationId == id && x.AuthorId == authorId && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Annotation not found or access denied.");
            ValidateAnnotationPayload(a.PageNo, request.Content);
            a.Content = request.Content.Trim();
            _repo.Update(a);
            await _repo.SaveChangeAsync();
            return await GetByIdAsync(id);
        }

        public async Task<AnnotationResponse> UpdateForSubmissionAsync(
            Guid submissionId,
            Guid id,
            Guid assistantId,
            UpdateAnnotationRequest request)
        {
            await EnsureOwnSubmissionAsync(submissionId, assistantId);

            var annotation = await _repo.GetAll()
                    .FirstOrDefaultAsync(x => x.AnnotationId == id
                        && x.PageTaskSubmissionId == submissionId
                        && x.AuthorId == assistantId
                        && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Annotation not found or access denied.");

            ValidateAnnotationPayload(annotation.PageNo, request.Content);
            annotation.Content = request.Content.Trim();
            _repo.Update(annotation);
            await _repo.SaveChangeAsync();
            return await GetByIdAsync(id);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var a = await _repo.GetAll().FirstOrDefaultAsync(x => x.AnnotationId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Annotation not found.");
            a.DeletedAt = DateTime.UtcNow;
            _repo.Update(a);
            await _repo.SaveChangeAsync();
        }

        public async Task SoftDeleteAsync(Guid id, Guid authorId)
        {
            var a = await _repo.GetAll()
                    .FirstOrDefaultAsync(x => x.AnnotationId == id && x.AuthorId == authorId && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Annotation not found or access denied.");
            a.DeletedAt = DateTime.UtcNow;
            _repo.Update(a);
            await _repo.SaveChangeAsync();
        }

        public async Task SoftDeleteForSubmissionAsync(Guid submissionId, Guid id, Guid assistantId)
        {
            await EnsureOwnSubmissionAsync(submissionId, assistantId);

            var annotation = await _repo.GetAll()
                    .FirstOrDefaultAsync(x => x.AnnotationId == id
                        && x.PageTaskSubmissionId == submissionId
                        && x.AuthorId == assistantId
                        && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Annotation not found or access denied.");

            annotation.DeletedAt = DateTime.UtcNow;
            _repo.Update(annotation);
            await _repo.SaveChangeAsync();
        }

        private static AnnotationResponse Map(Annotation a) => new()
        {
            AnnotationId = a.AnnotationId,
            ManuscriptId = a.ManuscriptId,
            PageTaskSubmissionId = a.PageTaskSubmissionId,
            ChapterId = a.Manuscript?.ChapterId ?? Guid.Empty,
            AuthorId = a.AuthorId,
            AuthorName = a.Author?.DisplayName ?? "",
            PageNo = a.PageNo,
            PositionX = a.PositionX,
            PositionY = a.PositionY,
            Content = a.Content,
            CreatedAt = a.CreatedAt
        };

        private static void ValidateAnnotationPayload(int pageNo, string? content)
        {
            if (pageNo <= 0)
                throw new ArgumentException("Page number must be greater than 0.");

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Annotation content is required.");
        }

        private async Task<PageTaskSubmission> EnsureOwnSubmissionAsync(Guid submissionId, Guid assistantId)
        {
            var submission = await _submissionRepo.GetAll()
                .Include(s => s.PageTask)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId && s.DeletedAt == null)
                ?? throw new KeyNotFoundException("Submission not found.");

            if (submission.PageTask.DeletedAt != null)
                throw new KeyNotFoundException("Page task not found.");

            if (submission.PageTask.AssistantId != assistantId)
                throw new UnauthorizedAccessException("Assistant can only annotate their own submissions.");

            return submission;
        }
    }
}
