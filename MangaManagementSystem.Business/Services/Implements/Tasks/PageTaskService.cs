using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Requests.Tasks;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.DTOs.Responses.Files;
using MangaManagementSystem.Business.DTOs.Responses.Tasks;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.Business.Services.Interfaces.Tasks;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MangaManagementSystem.Business.Services.Implements.Tasks;

public class PageTaskService : IPageTaskService
{
    private const int MaxActiveSubmissionAttempts = 3;

    private readonly IRepository<PageTask> _pageTaskRepository;
    private readonly IRepository<PageTaskSubmission> _submissionRepository;
    private readonly IRepository<Chapter> _chapterRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<FileAsset> _fileAssetRepository;
    private readonly IRepository<PageTaskReferenceFile> _pageTaskReferenceFileRepository;
    private readonly IRepository<SalaryRecord> _salaryRecordRepository;
    private readonly INotificationDispatchService _notificationDispatchService;
    private readonly IMapper _mapper;
    private readonly ILogger<PageTaskService> _logger;
    private readonly string _supabaseUrl;

    public PageTaskService(
        IRepository<PageTask> pageTaskRepository,
        IRepository<PageTaskSubmission> submissionRepository,
        IRepository<Chapter> chapterRepository,
        IRepository<User> userRepository,
        IRepository<FileAsset> fileAssetRepository,
        IRepository<PageTaskReferenceFile> pageTaskReferenceFileRepository,
        IRepository<SalaryRecord> salaryRecordRepository,
        INotificationDispatchService notificationDispatchService,
        IConfiguration configuration,
        IMapper mapper,
        ILogger<PageTaskService> logger)
    {
        _pageTaskRepository = pageTaskRepository;
        _submissionRepository = submissionRepository;
        _chapterRepository = chapterRepository;
        _userRepository = userRepository;
        _fileAssetRepository = fileAssetRepository;
        _pageTaskReferenceFileRepository = pageTaskReferenceFileRepository;
        _salaryRecordRepository = salaryRecordRepository;
        _notificationDispatchService = notificationDispatchService;
        _mapper = mapper;
        _logger = logger;
        _supabaseUrl = (configuration["Supabase:Url"] ?? string.Empty).TrimEnd('/');
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

        var assistant = await _userRepository.GetAll()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == request.AssistantId && x.DeletedAt == null);

        if (assistant == null)
            throw new KeyNotFoundException("Assistant not found.");

        if (assistant.Role.RoleName != UserRole.Assistant.ToString())
            throw new ArgumentException("Assigned user must have Assistant role.");

        var hasOverlappingActiveTask = await _pageTaskRepository.GetAll()
            .AnyAsync(x => x.ChapterId == request.ChapterId
                && x.DeletedAt == null
                && x.Status != PageTaskStatus.Approved
                && x.PageStart <= request.PageEnd
                && x.PageEnd >= request.PageStart);

        if (hasOverlappingActiveTask)
            throw new InvalidOperationException(
                "Page range overlaps with an active page task in this chapter.");

        var task = new PageTask
        {
            ChapterId = request.ChapterId,
            AssistantId = request.AssistantId,
            PageStart = request.PageStart,
            PageEnd = request.PageEnd,
            TaskType = string.IsNullOrWhiteSpace(request.TaskType) ? null : request.TaskType.Trim(),
            RatePerPage = request.RatePerPage,
            Description = request.Description?.Trim(),
            DueDate = request.DueDate,
            Status = PageTaskStatus.Assigned,
            CreatedAt = DateTime.UtcNow
        };

        var fileAssetIds = NormalizeFileAssetIds(request.ReferenceFileAssetIds);
        if (fileAssetIds.Count > 0)
        {
            await EnsureFileAssetsExistAsync(fileAssetIds);

            foreach (var fileAssetId in fileAssetIds)
            {
                task.ReferenceFiles.Add(new PageTaskReferenceFile
                {
                    FileAssetId = fileAssetId,
                    CreatedAt = task.CreatedAt
                });
            }
        }

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

        return tasks.Select(MapTask);
    }

    public async Task<IEnumerable<PageTaskResponse>> GetAssistantTasksAsync(Guid assistantId)
    {
        var tasks = await BaseTaskQuery()
            .Where(x => x.AssistantId == assistantId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapTask);
    }

    public async Task<PageTaskResponse> UpdateAsync(Guid mangakaId, Guid pageTaskId, UpdatePageTaskRequest request)
    {
        var task = await _pageTaskRepository.GetAll()
            .Include(x => x.Chapter)
                .ThenInclude(x => x.Series)
            .Include(x => x.Submissions.Where(s => s.DeletedAt == null))
            .FirstOrDefaultAsync(x => x.PageTaskId == pageTaskId && x.DeletedAt == null);

        if (task == null)
            throw new KeyNotFoundException("Page task not found.");

        if (task.Chapter.Series.MangakaId != mangakaId)
            throw new UnauthorizedAccessException("You can only update tasks for your own series.");

        var assistantChanged = request.AssistantId.HasValue && request.AssistantId.Value != task.AssistantId;
        var taskContentChanged = request.PageStart.HasValue
            || request.PageEnd.HasValue
            || request.RatePerPage.HasValue
            || request.Description != null
            || request.DueDate.HasValue;

        if (task.Status == PageTaskStatus.Approved)
            throw new InvalidOperationException("Approved page task cannot be updated.");

        if (taskContentChanged && task.Submissions.Any())
            throw new InvalidOperationException("Page task details cannot be updated after it has submissions.");

        if (!assistantChanged && !taskContentChanged)
            return await GetTaskResponseForMangakaAsync(mangakaId, pageTaskId);

        var now = DateTime.UtcNow;

        if (assistantChanged)
        {
            var newAssistantId = request.AssistantId!.Value;
            var assistant = await _userRepository.GetAll()
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == newAssistantId && x.DeletedAt == null);

            if (assistant == null)
                throw new KeyNotFoundException("Assistant not found.");

            if (assistant.Role.RoleName != UserRole.Assistant.ToString())
                throw new ArgumentException("Assigned user must have Assistant role.");

            if (task.Submissions.Any(x => x.Status == PageTaskSubmissionStatus.Submitted))
                throw new InvalidOperationException("Cannot reassign a page task while a submission is waiting for review.");

            foreach (var submission in task.Submissions.Where(x => x.DeletedAt == null))
            {
                submission.DeletedAt = now;
                _submissionRepository.Update(submission);
            }

            task.AssistantId = newAssistantId;
            task.Status = PageTaskStatus.Assigned;
            task.ApprovedAt = null;
        }

        var pageStart = request.PageStart ?? task.PageStart;
        var pageEnd = request.PageEnd ?? task.PageEnd;

        if (pageStart > pageEnd)
            throw new ArgumentException("PageStart must be less than or equal to PageEnd.");

        if (pageEnd > task.Chapter.TotalPages)
            throw new ArgumentException("Page range exceeds chapter total pages.");

        var pageRangeChanged = pageStart != task.PageStart || pageEnd != task.PageEnd;
        if (pageRangeChanged)
        {
            var hasOverlappingActiveTask = await _pageTaskRepository.GetAll()
                .AnyAsync(x => x.PageTaskId != pageTaskId
                    && x.ChapterId == task.ChapterId
                    && x.DeletedAt == null
                    && x.Status != PageTaskStatus.Approved
                    && x.PageStart <= pageEnd
                    && x.PageEnd >= pageStart);

            if (hasOverlappingActiveTask)
                throw new InvalidOperationException("Page range overlaps with an active page task in this chapter.");

            task.PageStart = pageStart;
            task.PageEnd = pageEnd;
        }

        if (request.Description != null)
            task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        if (request.RatePerPage.HasValue)
            task.RatePerPage = request.RatePerPage;

        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate;

        task.UpdatedAt = now;

        _pageTaskRepository.Update(task);
        await _pageTaskRepository.SaveChangeAsync();

        return await GetTaskResponseForMangakaAsync(mangakaId, pageTaskId);
    }

    public async Task<PageTaskResponse> AddReferenceFilesAsync(Guid mangakaId, Guid pageTaskId, AttachPageTaskReferenceFilesRequest request)
    {
        var task = await _pageTaskRepository.GetAll()
            .Include(x => x.Chapter)
                .ThenInclude(x => x.Series)
            .FirstOrDefaultAsync(x => x.PageTaskId == pageTaskId && x.DeletedAt == null);

        if (task == null)
            throw new KeyNotFoundException("Page task not found.");

        if (task.Chapter.Series.MangakaId != mangakaId)
            throw new UnauthorizedAccessException("You can only attach reference files to tasks for your own series.");

        var fileAssetIds = NormalizeFileAssetIds(request.FileAssetIds);
        if (fileAssetIds.Count == 0)
            throw new ArgumentException("At least one file asset is required.");

        await EnsureFileAssetsExistAsync(fileAssetIds);

        var existingFileAssetIds = await _pageTaskReferenceFileRepository.GetAll()
            .Where(x => x.PageTaskId == pageTaskId
                && x.DeletedAt == null
                && fileAssetIds.Contains(x.FileAssetId))
            .Select(x => x.FileAssetId)
            .ToListAsync();

        var existingSet = existingFileAssetIds.ToHashSet();
        var newReferenceFiles = fileAssetIds
            .Where(fileAssetId => !existingSet.Contains(fileAssetId))
            .Select(fileAssetId => new PageTaskReferenceFile
            {
                PageTaskId = pageTaskId,
                FileAssetId = fileAssetId,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (newReferenceFiles.Count > 0)
        {
            await _pageTaskReferenceFileRepository.AddRangeAsync(newReferenceFiles);
            await _pageTaskReferenceFileRepository.SaveChangeAsync();
        }

        return await GetTaskResponseForMangakaAsync(mangakaId, pageTaskId);
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

        var activeSubmissionAttemptCount = task.Submissions
            .Count(x => x.Status != PageTaskSubmissionStatus.Approved);

        if (activeSubmissionAttemptCount >= MaxActiveSubmissionAttempts)
            throw new InvalidOperationException("Đã hết lượt nộp.");

        var fileExists = await _fileAssetRepository.GetAll()
            .AnyAsync(x => x.FileAssetId == request.SubmittedFileAssetId && x.DeletedAt == null);

        if (!fileExists)
            throw new KeyNotFoundException("Submitted file asset not found.");

        var latestVersion = await _submissionRepository.GetAll()
            .Where(x => x.PageTaskId == task.PageTaskId)
            .MaxAsync(x => (int?)x.VersionNo) ?? 0;

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
        var approvedAt = DateTime.UtcNow;

        submission.Status = PageTaskSubmissionStatus.Approved;
        submission.Feedback = null;
        submission.ReviewedAt = approvedAt;

        task.Status = PageTaskStatus.Approved;
        task.ApprovedAt = approvedAt;
        task.UpdatedAt = approvedAt;

        var salaryExists = await _salaryRecordRepository.GetAll()
            .AnyAsync(x => x.PageTaskId == task.PageTaskId);

        if (!salaryExists)
        {
            var pages = task.PageEnd - task.PageStart + 1;
            var rateAtApproval = task.RatePerPage ?? 0m;

            await _salaryRecordRepository.AddAsync(new SalaryRecord
            {
                AssistantId = task.AssistantId,
                PageTaskId = task.PageTaskId,
                Pages = pages,
                RateAtApproval = rateAtApproval,
                Amount = pages * rateAtApproval,
                ApprovedAt = approvedAt,
                CreatedAt = approvedAt
            });
        }

        _submissionRepository.Update(submission);
        _pageTaskRepository.Update(task);
        await _pageTaskRepository.SaveChangeAsync();
        await TryNotifyAssistantReviewResultAsync(task, approved: true);

        return await GetTaskResponseForMangakaAsync(mangakaId, task.PageTaskId);
    }

    public async Task<PageTaskResponse> RejectSubmissionAsync(Guid mangakaId, Guid submissionId, ReviewPageTaskSubmissionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Feedback))
            throw new ArgumentException("Feedback is required when rejecting a submission.");

        var (task, submission) = await GetReviewTargetAsync(mangakaId, submissionId);

        submission.Status = PageTaskSubmissionStatus.Rejected;
        submission.Feedback = request.Feedback.Trim();
        submission.ReviewedAt = DateTime.UtcNow;

        task.Status = PageTaskStatus.InProgress;
        task.UpdatedAt = DateTime.UtcNow;

        _submissionRepository.Update(submission);
        _pageTaskRepository.Update(task);
        await _pageTaskRepository.SaveChangeAsync();
        await TryNotifyAssistantReviewResultAsync(task, approved: false, submission.Feedback);

        return await GetTaskResponseForMangakaAsync(mangakaId, task.PageTaskId);
    }

    private async Task TryNotifyAssistantReviewResultAsync(PageTask task, bool approved, string? feedback = null)
    {
        var taskLabel = string.IsNullOrWhiteSpace(task.TaskType)
            ? $"pages {task.PageStart}-{task.PageEnd}"
            : $"{task.TaskType} pages {task.PageStart}-{task.PageEnd}";

        var message = approved
            ? $"Your page task for {taskLabel} has been approved."
            : $"Your page task for {taskLabel} was rejected and needs revision."
                + (string.IsNullOrWhiteSpace(feedback) ? string.Empty : $" Feedback: {feedback}");

        var request = new NotificationDispatchRequest
        {
            Message = message.Length <= 1000 ? message : message[..1000]
        };

        try
        {
            var result = await _notificationDispatchService.DispatchToUsersAsync(
                request,
                new[] { task.AssistantId });

            if (result.Status == NotificationDispatchStatus.NoRecipients)
            {
                _logger.LogWarning(
                    "Page task {PageTaskId} review notification had no recipients: {Message}",
                    task.PageTaskId,
                    result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Page task {PageTaskId} was reviewed, but assistant notification dispatch failed.",
                task.PageTaskId);
        }
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

        return MapTask(task);
    }

    private async Task<PageTaskResponse> GetTaskResponseForAssistantAsync(Guid assistantId, Guid pageTaskId)
    {
        var task = await BaseTaskQuery()
            .FirstOrDefaultAsync(x => x.PageTaskId == pageTaskId && x.AssistantId == assistantId);

        if (task == null)
            throw new KeyNotFoundException("Page task not found.");

        return MapTask(task);
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
            .Include(x => x.ReferenceFiles.Where(rf => rf.DeletedAt == null))
                .ThenInclude(x => x.FileAsset)
            .Where(x => x.DeletedAt == null);
    }

    private PageTaskResponse MapTask(PageTask task)
    {
        var response = _mapper.Map<PageTaskResponse>(task);
        response.Submissions = task.Submissions
            .Where(s => s.DeletedAt == null)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(MapSubmission)
            .ToList();

        response.ReferenceFiles = task.ReferenceFiles
            .Where(rf => rf.DeletedAt == null && rf.FileAsset.DeletedAt == null)
            .OrderBy(rf => rf.CreatedAt)
            .Select(rf => MapFileAsset(rf.FileAsset))
            .ToList();

        return response;
    }

    private PageTaskSubmissionResponse MapSubmission(PageTaskSubmission submission) => new()
    {
        SubmissionId = submission.SubmissionId,
        PageTaskId = submission.PageTaskId,
        VersionNo = submission.VersionNo,
        SubmittedFileAssetId = submission.SubmittedFileAssetId,
        SubmittedFileAssetUrl = MapFileAssetUrl(submission.SubmittedFileAsset),
        OriginalFileName = submission.SubmittedFileAsset?.OriginalFileName,
        ObjectPath = submission.SubmittedFileAsset?.ObjectPath,
        Status = submission.Status,
        Note = submission.Note,
        Feedback = submission.Feedback,
        SubmittedAt = submission.SubmittedAt,
        ReviewedAt = submission.ReviewedAt
    };

    private FileAssetResponse MapFileAsset(FileAsset fileAsset) => new()
    {
        FileAssetId = fileAsset.FileAssetId,
        BucketName = fileAsset.BucketName,
        ObjectPath = fileAsset.ObjectPath,
        OriginalFileName = fileAsset.OriginalFileName,
        StoredFileName = fileAsset.StoredFileName,
        Extension = fileAsset.Extension,
        FileSizeBytes = fileAsset.FileSizeBytes,
        MimeType = fileAsset.MimeType,
        PublicUrl = MapFileAssetUrl(fileAsset)
    };

    private string? MapFileAssetUrl(FileAsset? fileAsset)
    {
        if (fileAsset == null || fileAsset.DeletedAt != null || string.IsNullOrEmpty(_supabaseUrl))
            return null;

        return $"{_supabaseUrl}/storage/v1/object/public/{fileAsset.BucketName}/{fileAsset.ObjectPath}";
    }

    private async Task EnsureFileAssetsExistAsync(IReadOnlyCollection<Guid> fileAssetIds)
    {
        var existingFileAssetIds = await _fileAssetRepository.GetAll()
            .Where(fileAsset => fileAssetIds.Contains(fileAsset.FileAssetId) && fileAsset.DeletedAt == null)
            .Select(fileAsset => fileAsset.FileAssetId)
            .ToListAsync();

        if (existingFileAssetIds.Count != fileAssetIds.Count)
        {
            var missingFileAssetIds = fileAssetIds.Except(existingFileAssetIds).ToList();
            throw new KeyNotFoundException($"File asset not found: {string.Join(", ", missingFileAssetIds)}.");
        }
    }

    private static IReadOnlyCollection<Guid> NormalizeFileAssetIds(IEnumerable<Guid>? fileAssetIds)
        => (fileAssetIds ?? Array.Empty<Guid>())
            .Where(fileAssetId => fileAssetId != Guid.Empty)
            .Distinct()
            .ToList();

}
