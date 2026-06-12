using MangaManagementSystem.Business.DTOs.Requests.Tasks;
using MangaManagementSystem.Business.DTOs.Responses.Tasks;
using MangaManagementSystem.Business.Services.Interfaces.Tasks;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Tasks
{
    public class PageTaskSubmissionService : IPageTaskSubmissionService
    {
        private readonly IRepository<PageTaskSubmission> _repo;

        public PageTaskSubmissionService(IRepository<PageTaskSubmission> repo) => _repo = repo;

        public async Task<IEnumerable<SubmissionResponse>> GetByTaskAsync(Guid pageTaskId)
            => await _repo.GetAll().Where(s => s.PageTaskId == pageTaskId && s.DeletedAt == null).Select(s => Map(s)).ToListAsync();

        public async Task<SubmissionResponse> GetByIdAsync(Guid id)
        {
            var s = await _repo.GetAll().FirstOrDefaultAsync(x => x.SubmissionId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Submission not found.");
            return Map(s);
        }

        public async Task<SubmissionResponse> CreateAsync(Guid assistantId, CreateSubmissionRequest request)
        {
            var submission = new PageTaskSubmission
            {
                PageTaskId = request.PageTaskId,
                VersionNo = request.VersionNo,
                SubmittedFileAssetId = request.SubmittedFileAssetId,
                Note = request.Note,
                Status = PageTaskSubmissionStatus.Submitted,
                SubmittedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(submission);
            await _repo.SaveChangeAsync();
            return Map(submission);
        }

        public async Task<SubmissionResponse> UpdateAsync(Guid id, UpdateSubmissionRequest request, Guid reviewerId)
        {
            var s = await _repo.GetAll().FirstOrDefaultAsync(x => x.SubmissionId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Submission not found.");
            if (Enum.TryParse<PageTaskSubmissionStatus>(request.Status, out var status)) s.Status = status;
            s.RejectReason = request.RejectReason;
            s.ReviewedAt = DateTime.UtcNow;
            _repo.Update(s);
            await _repo.SaveChangeAsync();
            return Map(s);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var s = await _repo.GetAll().FirstOrDefaultAsync(x => x.SubmissionId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Submission not found.");
            s.DeletedAt = DateTime.UtcNow;
            _repo.Update(s);
            await _repo.SaveChangeAsync();
        }

        private static SubmissionResponse Map(PageTaskSubmission s) => new()
        {
            SubmissionId = s.SubmissionId, PageTaskId = s.PageTaskId, VersionNo = s.VersionNo,
            SubmittedFileAssetId = s.SubmittedFileAssetId, Status = s.Status.ToString(),
            Note = s.Note, RejectReason = s.RejectReason, SubmittedAt = s.SubmittedAt, ReviewedAt = s.ReviewedAt
        };
    }
}
