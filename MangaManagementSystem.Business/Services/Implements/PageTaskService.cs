using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace MangaManagementSystem.Business.Services.Implements
{
    public class PageTaskService : IPageTaskService
    {
        private const string ActiveStatus = "Active";
        private const string AssistantRoleName = "Assistant";

        private readonly IRepository<PageTask> _pageTaskRepository;
        private readonly IRepository<PageTaskSubmission> _submissionRepository;
        private readonly IRepository<Chapter> _chapterRepository;
        private readonly IRepository<Manuscript> _manuscriptRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserAssignment> _userAssignmentRepository;
        private readonly IRepository<FileAsset> _fileAssetRepository;
        private readonly IMapper _mapper;

        public PageTaskService(
            IRepository<PageTask> pageTaskRepository,
            IRepository<PageTaskSubmission> submissionRepository,
            IRepository<Chapter> chapterRepository,
            IRepository<Manuscript> manuscriptRepository,
            IRepository<User> userRepository,
            IRepository<UserAssignment> userAssignmentRepository,
            IRepository<FileAsset> fileAssetRepository,
            IMapper mapper)
        {
            _pageTaskRepository = pageTaskRepository;
            _submissionRepository = submissionRepository;
            _chapterRepository = chapterRepository;
            _manuscriptRepository = manuscriptRepository;
            _userRepository = userRepository;
            _userAssignmentRepository = userAssignmentRepository;
            _fileAssetRepository = fileAssetRepository;
            _mapper = mapper;
        }

        public async Task<PageTaskResponse> CreateAsync(Guid mangakaId, CreatePageTaskRequest request)
        {
            if (request.PageStart > request.PageEnd)
                throw new ArgumentException("Trang bat dau phai nho hon hoac bang trang ket thuc.");

            var chapter = await _chapterRepository.GetAll()
                .Include(x => x.Series)
                .FirstOrDefaultAsync(x => x.ChapterId == request.ChapterId);

            if (chapter == null)
                throw new KeyNotFoundException("Chapter khong ton tai.");

            if (chapter.Series.MangakaId != mangakaId)
                throw new UnauthorizedAccessException("Ban khong co quyen giao task cho chapter nay.");

            if (request.PageEnd > chapter.TotalPages)
                throw new ArgumentException("Khoang trang vuot qua tong so trang cua chapter.");

            var manuscriptExists = await _manuscriptRepository.GetAll()
                .AnyAsync(x => x.ManuscriptId == request.ManuscriptId && x.ChapterId == request.ChapterId);

            if (!manuscriptExists)
                throw new ArgumentException("Manuscript khong thuoc chapter da chon.");

            var assistant = await _userRepository.GetAll()
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == request.AssistantId);

            if (assistant == null)
                throw new KeyNotFoundException("Assistant khong ton tai.");

            if (!string.Equals(assistant.Status, ActiveStatus, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(assistant.Role.RoleName, AssistantRoleName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Nguoi duoc giao task phai la Assistant dang hoat dong.");

            var isAssignedAssistant = await _userAssignmentRepository.GetAll()
                .AnyAsync(x => x.FromUserId == mangakaId && x.ToUserId == request.AssistantId && x.Status);

            if (!isAssignedAssistant)
                throw new UnauthorizedAccessException("Assistant nay chua duoc gan lam viec voi Mangaka hien tai.");

            var pageTask = new PageTask
            {
                ChapterId = request.ChapterId,
                ManuscriptId = request.ManuscriptId,
                AssistantId = request.AssistantId,
                PageStart = request.PageStart,
                PageEnd = request.PageEnd,
                TaskType = request.TaskType.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                DueDate = request.DueDate,
                Status = PageTaskStatus.Assigned,
                CreatedAt = DateTime.UtcNow
            };

            await _pageTaskRepository.AddAsync(pageTask);
            await _pageTaskRepository.SaveChangeAsync();

            return await GetTaskResponseAsync(pageTask.PageTaskId);
        }

        public async Task<IReadOnlyCollection<PageTaskResponse>> GetForMangakaAsync(Guid mangakaId)
        {
            var tasks = await BaseTaskQuery()
                .Where(x => x.Chapter.Series.MangakaId == mangakaId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IReadOnlyCollection<PageTaskResponse>>(tasks);
        }

        public async Task<IReadOnlyCollection<PageTaskResponse>> GetForAssistantAsync(Guid assistantId)
        {
            var tasks = await BaseTaskQuery()
                .Where(x => x.AssistantId == assistantId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IReadOnlyCollection<PageTaskResponse>>(tasks);
        }

        public async Task<PageTaskSubmissionResponse> SubmitAsync(Guid assistantId, Guid pageTaskId, SubmitPageTaskRequest request)
        {
            var pageTask = await _pageTaskRepository.GetAll()
                .Include(x => x.Submissions)
                .FirstOrDefaultAsync(x => x.PageTaskId == pageTaskId);

            if (pageTask == null)
                throw new KeyNotFoundException("Task khong ton tai.");

            if (pageTask.AssistantId != assistantId)
                throw new UnauthorizedAccessException("Ban khong co quyen submit task nay.");

            if (pageTask.Status == PageTaskStatus.Approved)
                throw new InvalidOperationException("Task da duoc approve, khong the submit tiep.");

            var fileAsset = await _fileAssetRepository.GetAll()
                .FirstOrDefaultAsync(x => x.FileAssetId == request.SubmittedFileAssetId);

            if (fileAsset == null)
                throw new KeyNotFoundException("File submit khong ton tai.");

            if (fileAsset.UploadedBy != assistantId)
                throw new UnauthorizedAccessException("Ban chi duoc submit file do chinh minh upload.");

            var nextVersionNo = pageTask.Submissions.Any()
                ? pageTask.Submissions.Max(x => x.VersionNo) + 1
                : 1;

            var submission = new PageTaskSubmission
            {
                PageTaskId = pageTask.PageTaskId,
                VersionNo = nextVersionNo,
                SubmittedFileAssetId = request.SubmittedFileAssetId,
                Status = PageTaskSubmissionStatus.Submitted,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                SubmittedAt = DateTime.UtcNow
            };

            pageTask.Status = PageTaskStatus.Submitted;
            pageTask.UpdatedAt = DateTime.UtcNow;

            await _submissionRepository.AddAsync(submission);
            await _submissionRepository.SaveChangeAsync();

            return await GetSubmissionResponseAsync(submission.SubmissionId);
        }

        public async Task<PageTaskSubmissionResponse> ApproveSubmissionAsync(Guid mangakaId, Guid submissionId)
        {
            var submission = await GetSubmissionForReviewAsync(mangakaId, submissionId);

            if (submission.Status != PageTaskSubmissionStatus.Submitted)
                throw new InvalidOperationException("Submission nay da duoc review.");

            submission.Status = PageTaskSubmissionStatus.Approved;
            submission.ReviewedAt = DateTime.UtcNow;
            submission.RejectReason = null;
            submission.PageTask.Status = PageTaskStatus.Approved;
            submission.PageTask.ApprovedAt = DateTime.UtcNow;
            submission.PageTask.UpdatedAt = DateTime.UtcNow;

            RejectOtherPendingSubmissions(submission);

            await _submissionRepository.SaveChangeAsync();

            return _mapper.Map<PageTaskSubmissionResponse>(submission);
        }

        public async Task<PageTaskSubmissionResponse> RejectSubmissionAsync(Guid mangakaId, Guid submissionId, RejectPageTaskSubmissionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RejectReason))
                throw new ArgumentException("Ly do reject la bat buoc.");

            var submission = await GetSubmissionForReviewAsync(mangakaId, submissionId);

            if (submission.Status != PageTaskSubmissionStatus.Submitted)
                throw new InvalidOperationException("Submission nay da duoc review.");

            submission.Status = PageTaskSubmissionStatus.Rejected;
            submission.RejectReason = request.RejectReason.Trim();
            submission.ReviewedAt = DateTime.UtcNow;
            submission.PageTask.Status = PageTaskStatus.Rejected;
            submission.PageTask.UpdatedAt = DateTime.UtcNow;

            await _submissionRepository.SaveChangeAsync();

            return _mapper.Map<PageTaskSubmissionResponse>(submission);
        }

        private IQueryable<PageTask> BaseTaskQuery()
        {
            return _pageTaskRepository.GetAll()
                .Include(x => x.Chapter)
                    .ThenInclude(x => x.Series)
                .Include(x => x.Assistant)
                .Include(x => x.Submissions)
                    .ThenInclude(x => x.SubmittedFileAsset);
        }

        private async Task<PageTaskResponse> GetTaskResponseAsync(Guid pageTaskId)
        {
            var pageTask = await BaseTaskQuery()
                .FirstAsync(x => x.PageTaskId == pageTaskId);

            return _mapper.Map<PageTaskResponse>(pageTask);
        }

        private async Task<PageTaskSubmissionResponse> GetSubmissionResponseAsync(Guid submissionId)
        {
            var submission = await _submissionRepository.GetAll()
                .Include(x => x.SubmittedFileAsset)
                .FirstAsync(x => x.SubmissionId == submissionId);

            return _mapper.Map<PageTaskSubmissionResponse>(submission);
        }

        private async Task<PageTaskSubmission> GetSubmissionForReviewAsync(Guid mangakaId, Guid submissionId)
        {
            var submission = await _submissionRepository.GetAll()
                .Include(x => x.SubmittedFileAsset)
                .Include(x => x.PageTask)
                    .ThenInclude(x => x.Chapter)
                        .ThenInclude(x => x.Series)
                .Include(x => x.PageTask)
                    .ThenInclude(x => x.Submissions)
                .FirstOrDefaultAsync(x => x.SubmissionId == submissionId);

            if (submission == null)
                throw new KeyNotFoundException("Submission khong ton tai.");

            if (submission.PageTask.Chapter.Series.MangakaId != mangakaId)
                throw new UnauthorizedAccessException("Ban khong co quyen review submission nay.");

            return submission;
        }

        private static void RejectOtherPendingSubmissions(PageTaskSubmission approvedSubmission)
        {
            foreach (var submission in approvedSubmission.PageTask.Submissions)
            {
                if (submission.SubmissionId == approvedSubmission.SubmissionId ||
                    submission.Status != PageTaskSubmissionStatus.Submitted)
                {
                    continue;
                }

                submission.Status = PageTaskSubmissionStatus.Rejected;
                submission.RejectReason = "Da co submission khac duoc approve.";
                submission.ReviewedAt = DateTime.UtcNow;
            }
        }

    }
}
