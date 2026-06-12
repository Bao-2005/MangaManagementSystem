using MangaManagementSystem.Business.DTOs.Requests.Manuscripts;
using MangaManagementSystem.Business.DTOs.Responses.Manuscripts;
using MangaManagementSystem.Business.Services.Interfaces.Manuscripts;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Manuscripts
{
    public class ManuscriptService : IManuscriptService
    {
        private readonly IRepository<Manuscript> _repo;
        private readonly IRepository<PageTask> _pageTaskRepo;
        private readonly IRepository<Annotation> _annotationRepo;

        public ManuscriptService(IRepository<Manuscript> repo, IRepository<PageTask> pageTaskRepo, IRepository<Annotation> annotationRepo)
        {
            _repo = repo;
            _pageTaskRepo = pageTaskRepo;
            _annotationRepo = annotationRepo;
        }

        public async Task<IEnumerable<ManuscriptResponse>> GetByChapterAsync(Guid chapterId)
            => await _repo.GetAll()
                .Include(m => m.Reviewer)
                .Where(m => m.ChapterId == chapterId && m.DeletedAt == null)
                .Select(m => Map(m)).ToListAsync();

        public async Task<ManuscriptResponse> GetByIdAsync(Guid id)
        {
            var m = await _repo.GetAll().Include(m => m.Reviewer)
                .FirstOrDefaultAsync(x => x.ManuscriptId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Manuscript not found.");
            return Map(m);
        }

        public async Task<ManuscriptResponse> CreateAsync(Guid mangakaId, CreateManuscriptRequest request)
        {
            // BR-04: all PageTasks for this chapter must be Approved
            var hasUnapproved = await _pageTaskRepo.GetAll()
                .AnyAsync(pt => pt.ChapterId == request.ChapterId && pt.DeletedAt == null
                                && pt.Status != DataAccess.Entities.Enums.PageTaskStatus.Approved);
            if (hasUnapproved)
                throw new InvalidOperationException("All page tasks must be approved before submitting a manuscript.");

            var lastVersion = await _repo.GetAll()
                .Where(m => m.ChapterId == request.ChapterId && m.DeletedAt == null)
                .MaxAsync(m => (int?)m.VersionNo) ?? 0;

            var manuscript = new Manuscript
            {
                ChapterId = request.ChapterId,
                FileUrl = request.FileUrl,
                Status = "Submitted",
                VersionNo = lastVersion + 1,
                RevisionCount = 0,
                SubmittedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(manuscript);
            await _repo.SaveChangeAsync();
            return Map(manuscript);
        }

        public async Task<ManuscriptResponse> UpdateAsync(Guid id, UpdateManuscriptRequest request, Guid reviewerId)
        {
            var m = await _repo.GetAll().FirstOrDefaultAsync(x => x.ManuscriptId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Manuscript not found.");
            if (request.Status != null) m.Status = request.Status;
            if (request.Feedback != null) m.Feedback = request.Feedback;
            m.ReviewedBy = reviewerId;
            m.ReviewedAt = DateTime.UtcNow;
            if (request.Status == "Approved") m.ApprovedAt = DateTime.UtcNow;
            if (request.Status == "RevisionRequired") m.RevisionCount++;
            _repo.Update(m);
            await _repo.SaveChangeAsync();
            return await GetByIdAsync(id);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var m = await _repo.GetAll().Include(m => m.Annotations)
                .FirstOrDefaultAsync(x => x.ManuscriptId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Manuscript not found.");
            var now = DateTime.UtcNow;
            m.DeletedAt = now;
            foreach (var a in m.Annotations.Where(a => a.DeletedAt == null)) a.DeletedAt = now;
            _repo.Update(m);
            await _repo.SaveChangeAsync();
        }

        private static ManuscriptResponse Map(Manuscript m) => new()
        {
            ManuscriptId = m.ManuscriptId, ChapterId = m.ChapterId, VersionNo = m.VersionNo,
            FileUrl = m.FileUrl, Status = m.Status, Feedback = m.Feedback, RevisionCount = m.RevisionCount,
            SubmittedAt = m.SubmittedAt, ReviewedBy = m.ReviewedBy,
            ReviewerName = m.Reviewer?.DisplayName, ReviewedAt = m.ReviewedAt, ApprovedAt = m.ApprovedAt
        };
    }
}
