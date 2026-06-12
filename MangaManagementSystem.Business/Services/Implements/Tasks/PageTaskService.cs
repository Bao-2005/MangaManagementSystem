using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MangaManagementSystem.Business.DTOs.Requests.Tasks;
using MangaManagementSystem.Business.DTOs.Responses.Tasks;
using MangaManagementSystem.Business.Services.Interfaces.Tasks;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Tasks;

    public class PageTaskService : IPageTaskService
    {
    private readonly IRepository<PageTask> _pageTaskRepository;
    private readonly IRepository<PageTaskSubmission> _submissionRepository;
    private readonly IRepository<Chapter> _chapterRepository;
    private readonly IRepository<Manuscript> _manuscriptRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<FileAsset> _fileAssetRepository;
    private readonly IMapper _mapper;

    public PageTaskService(
        IRepository<PageTask> pageTaskRepository,
        IRepository<PageTaskSubmission> submissionRepository,
        IRepository<Chapter> chapterRepository,
        IRepository<Manuscript> manuscriptRepository,
        IRepository<User> userRepository,
        IRepository<FileAsset> fileAssetRepository,
        IMapper mapper)
    {
        _pageTaskRepository = pageTaskRepository;
        _submissionRepository = submissionRepository;
        _chapterRepository = chapterRepository;
        _manuscriptRepository = manuscriptRepository;
        _userRepository = userRepository;
        _fileAssetRepository = fileAssetRepository;
        _mapper = mapper;
    }

    public async Task<PageTaskResponse> CreateAsync(Guid mangakaId, CreatePageTaskRequest request)
    {
        if (request.PageStart > request.PageEnd)
            throw new ArgumentException("PageStart must be less than or equal to PageEnd.");

        var chapter = await _chapterRepository.GetAll()
            .Include(x => x.Series)
            .FirstOrDefaultAsync(x => x.ChapterId == request.ChapterId && x.DeletedAt == null);

        if (chapter == null)
            throw new KeyNotFoundException("Chapter not found.");

        if (chapter.Series.MangakaId != mangakaId)
            throw new UnauthorizedAccessException("You can only assign tasks for your own series.");

        if (request.PageEnd > chapter.TotalPages)
            throw new ArgumentException("Page range exceeds chapter total pages.");

        var manuscript = await _manuscriptRepository.GetAll()
            .Where(x => x.ChapterId == request.ChapterId && x.DeletedAt == null)
            .OrderByDescending(x => x.VersionNo)
            .ThenByDescending(x => x.SubmittedAt)
            .FirstOrDefaultAsync();

        if (manuscript == null)
            throw new KeyNotFoundException("Manuscript not found for this chapter.");

        var assistant = await _userRepository.GetAll()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == request.AssistantId && x.DeletedAt == null);

        if (assistant == null)
            throw new KeyNotFoundException("Assistant not found.");

        if (assistant.Role.RoleName != UserRole.Assistant.ToString())
            throw new ArgumentException("Assigned user must have Assistant role.");
    

        var task = new PageTask
        {
            ChapterId = request.ChapterId,
            ManuscriptId = manuscript.ManuscriptId,
            AssistantId = request.AssistantId,
            PageStart = request.PageStart,
            PageEnd = request.PageEnd,
            TaskType = request.TaskType.Trim(),
            Description = request.Description?.Trim(),
            DueDate = request.DueDate,
            Status = PageTaskStatus.Assigned,
            CreatedAt = DateTime.UtcNow
        };

        await _pageTaskRepository.AddAsync(task);
        await _pageTaskRepository.SaveChangeAsync();

        return await GetTaskResponseForMangakaAsync(mangakaId, task.PageTaskId);
    }

    public async Task<IEnumerable<PageTaskResponse>> GetMangakaTasksAsync(Guid mangakaId)
    {
        var tasks = await BaseTaskQuery()
            .Where(x => x.Chapter.Series.MangakaId == mangakaId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<PageTaskResponse>>(tasks);
    }

    public async Task<IEnumerable<PageTaskResponse>> GetAssistantTasksAsync(Guid assistantId)
    {
        var tasks = await BaseTaskQuery()
            .Where(x => x.AssistantId == assistantId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<PageTaskResponse>>(tasks);
    }

    public async Task<PageTaskResponse> SubmitAsync(Guid assistantId, Guid pageTaskId, SubmitPageTaskRequest request)
    {
        var task = await _pageTaskRepository.GetAll()
            .Include(x => x.Submissions.Where(s => s.DeletedAt == null))
            .FirstOrDefaultAsync(x => x.PageTaskId == pageTaskId && x.DeletedAt == null);

        if (task == null)
            throw new KeyNotFoundException("Page task not found.");

        if (task.AssistantId != assistantId)
            throw new UnauthorizedAccessException("You can only submit your own assigned tasks.");

        if (task.Status == PageTaskStatus.Approved)
            throw new InvalidOperationException("Approved tasks cannot be submitted again.");

        if (task.Submissions.Any(x => x.Status == PageTaskSubmissionStatus.Submitted))
            throw new InvalidOperationException("This task already has a submission waiting for review.");

        var fileExists = await _fileAssetRepository.GetAll()
            .AnyAsync(x => x.FileAssetId == request.SubmittedFileAssetId && x.DeletedAt == null);

        if (!fileExists)
            throw new KeyNotFoundException("Submitted file asset not found.");

        var latestVersion = task.Submissions.Any()
            ? task.Submissions.Max(x => x.VersionNo)
            : 0;

        var submission = new PageTaskSubmission
        {
            PageTaskId = task.PageTaskId,
            VersionNo = latestVersion + 1,
            SubmittedFileAssetId = request.SubmittedFileAssetId,
            Status = PageTaskSubmissionStatus.Submitted,
            Note = request.Note?.Trim(),
            SubmittedAt = DateTime.UtcNow
        };

        task.Status = PageTaskStatus.Completed;
        task.UpdatedAt = DateTime.UtcNow;

        await _submissionRepository.AddAsync(submission);
        _pageTaskRepository.Update(task);
        await _pageTaskRepository.SaveChangeAsync();

        return await GetTaskResponseForAssistantAsync(assistantId, task.PageTaskId);
        }

    public async Task<PageTaskResponse> ApproveSubmissionAsync(Guid mangakaId, Guid submissionId)
        {
        var (task, submission) = await GetReviewTargetAsync(mangakaId, submissionId);

        submission.Status = PageTaskSubmissionStatus.Approved;
        submission.RejectReason = null;
        submission.ReviewedAt = DateTime.UtcNow;

        task.Status = PageTaskStatus.Approved;
        task.ApprovedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

        _submissionRepository.Update(submission);
        _pageTaskRepository.Update(task);
        await _pageTaskRepository.SaveChangeAsync();

        return await GetTaskResponseForMangakaAsync(mangakaId, task.PageTaskId);
    }

    public async Task<PageTaskResponse> RejectSubmissionAsync(Guid mangakaId, Guid submissionId, ReviewPageTaskSubmissionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RejectReason))
            throw new ArgumentException("RejectReason is required when rejecting a submission.");

        var (task, submission) = await GetReviewTargetAsync(mangakaId, submissionId);

        submission.Status = PageTaskSubmissionStatus.Rejected;
        submission.RejectReason = request.RejectReason.Trim();
        submission.ReviewedAt = DateTime.UtcNow;

        task.Status = PageTaskStatus.InProgress;
        task.UpdatedAt = DateTime.UtcNow;

        _submissionRepository.Update(submission);
        _pageTaskRepository.Update(task);
        await _pageTaskRepository.SaveChangeAsync();

        return await GetTaskResponseForMangakaAsync(mangakaId, task.PageTaskId);
    }

    private async Task<(PageTask Task, PageTaskSubmission Submission)> GetReviewTargetAsync(Guid mangakaId, Guid submissionId)
    {
        var submission = await _submissionRepository.GetAll()
            .Include(x => x.PageTask)
                .ThenInclude(x => x.Chapter)
                    .ThenInclude(x => x.Series)
            .FirstOrDefaultAsync(x => x.SubmissionId == submissionId && x.DeletedAt == null);

        if (submission == null)
            throw new KeyNotFoundException("Submission not found.");

        var task = submission.PageTask;

        if (task.DeletedAt != null)
            throw new KeyNotFoundException("Page task not found.");

        if (task.Chapter.Series.MangakaId != mangakaId)
            throw new UnauthorizedAccessException("You can only review tasks for your own series.");

        if (submission.Status != PageTaskSubmissionStatus.Submitted)
            throw new InvalidOperationException("Only submitted submissions can be reviewed.");

        return (task, submission);
    }

    private async Task<PageTaskResponse> GetTaskResponseForMangakaAsync(Guid mangakaId, Guid pageTaskId)
    {
        var task = await BaseTaskQuery()
            .FirstOrDefaultAsync(x => x.PageTaskId == pageTaskId && x.Chapter.Series.MangakaId == mangakaId);

        if (task == null)
            throw new KeyNotFoundException("Page task not found.");

        return _mapper.Map<PageTaskResponse>(task);
        }

    private async Task<PageTaskResponse> GetTaskResponseForAssistantAsync(Guid assistantId, Guid pageTaskId)
        {
        var task = await BaseTaskQuery()
            .FirstOrDefaultAsync(x => x.PageTaskId == pageTaskId && x.AssistantId == assistantId);

        if (task == null)
            throw new KeyNotFoundException("Page task not found.");

        return _mapper.Map<PageTaskResponse>(task);
        }

    private IQueryable<PageTask> BaseTaskQuery()
        {
        return _pageTaskRepository.GetAll()
            .AsNoTracking()
            .Include(x => x.Assistant)
            .Include(x => x.Chapter)
                .ThenInclude(x => x.Series)
            .Include(x => x.Submissions.Where(s => s.DeletedAt == null))
                .ThenInclude(x => x.SubmittedFileAsset)
            .Where(x => x.DeletedAt == null);
    }

}

