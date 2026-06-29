using MangaManagementSystem.Business.DTOs.Requests.Tasks;
using MangaManagementSystem.Business.DTOs.Responses.Tasks;
using MangaManagementSystem.Business.Services.Interfaces.Tasks;
using MangaManagementSystem.Business.Exceptions;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Tasks
{
    public class AnnotationService : IAnnotationService
    {
        private readonly IRepository<Annotation> _repo;
        private readonly IRepository<PageTaskSubmission> _submissionRepo;
        private readonly IRepository<Manuscript> _manuscriptRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<UserAssignment> _assignmentRepo;

        public AnnotationService(
            IRepository<Annotation> repo,
            IRepository<PageTaskSubmission> submissionRepo,
            IRepository<Manuscript> manuscriptRepo,
            IRepository<User> userRepo,
            IRepository<UserAssignment> assignmentRepo)
        {
            _repo = repo;
            _submissionRepo = submissionRepo;
            _manuscriptRepo = manuscriptRepo;
            _userRepo = userRepo;
            _assignmentRepo = assignmentRepo;
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

        public async Task<AnnotationResponse> CreateAsync(Guid authorId, Guid manuscriptId, CreateAnnotationRequest request)
        {
            ValidateAnnotationPayload(request.PageNo, request.PositionX, request.PositionY, request.Content);
            var manuscript = await EnsureCanAnnotateManuscriptAsync(authorId, manuscriptId);
            EnsurePageNoWithinChapter(request.PageNo, manuscript.Chapter.TotalPages);

            var annotation = new Annotation
            {
                ManuscriptId = manuscriptId,
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

            if (!submission.PageTask.ManuscriptId.HasValue)
                throw new InvalidOperationException("This page task is not linked to a manuscript yet.");

            ValidateAnnotationPayload(request.PageNo, request.PositionX, request.PositionY, request.Content);
            EnsurePageNoWithinTaskRange(request.PageNo, submission.PageTask.PageStart, submission.PageTask.PageEnd);

            var annotation = new Annotation
            {
                ManuscriptId = submission.PageTask.ManuscriptId.Value,
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
            ValidateAnnotationPayload(a.PageNo, a.PositionX, a.PositionY, request.Content);
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

            ValidateAnnotationPayload(annotation.PageNo, annotation.PositionX, annotation.PositionY, request.Content);
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

        private static void ValidateAnnotationPayload(int pageNo, decimal positionX, decimal positionY, string? content)
        {
            if (pageNo <= 0)
                throw new ArgumentException("Page number must be greater than 0.");

            if (positionX < 0 || positionX > 1 || positionY < 0 || positionY > 1)
                throw new ArgumentException("Annotation position must be between 0 and 1.");

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Annotation content is required.");

            if (content.Trim().Length > 2000)
                throw new ArgumentException("Annotation content must not exceed 2000 characters.");
        }

        private static void EnsurePageNoWithinChapter(int pageNo, int totalPages)
        {
            if (totalPages <= 0)
                throw new InvalidOperationException("Chapter total pages is not configured.");

            if (pageNo > totalPages)
                throw new ArgumentException("Page number must not exceed chapter total pages.");
        }

        private static void EnsurePageNoWithinTaskRange(int pageNo, int pageStart, int pageEnd)
        {
            if (pageNo < pageStart || pageNo > pageEnd)
                throw new ArgumentException("Page number must be within the assigned page task range.");
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

        private async Task<Manuscript> EnsureCanAnnotateManuscriptAsync(Guid authorId, Guid manuscriptId)
        {
            var manuscript = await _manuscriptRepo.GetAll()
                .Include(m => m.Chapter)
                    .ThenInclude(c => c.Series)
                .FirstOrDefaultAsync(m => m.ManuscriptId == manuscriptId && m.DeletedAt == null)
                ?? throw new KeyNotFoundException("Manuscript not found.");

            if (manuscript.Chapter.DeletedAt != null || manuscript.Chapter.Series.DeletedAt != null)
                throw new KeyNotFoundException("Manuscript not found.");

            var user = await _userRepo.GetAll()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == authorId && u.DeletedAt == null)
                ?? throw new UnauthorizedAccessException("User not found or inactive.");

            if (string.Equals(user.Role.RoleName, UserRole.Mangaka.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (manuscript.Chapter.Series.MangakaId == authorId)
                    return manuscript;
            }
            else if (string.Equals(user.Role.RoleName, UserRole.TantouEditor.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var isAssignedEditor = await _assignmentRepo.GetAll()
                    .AnyAsync(a => a.FromUserId == authorId
                        && a.ToUserId == manuscript.Chapter.Series.MangakaId
                        && a.DeletedAt == null
                        && a.UnassignedAt == null);

                if (isAssignedEditor)
                    return manuscript;
            }

            throw new ForbiddenAccessException("You do not have permission to annotate this manuscript.");
        }
    }
}
