using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements
{
    public class PageTaskService : IPageTaskService
    {
        private readonly IRepository<PageTask> _repo;

        public PageTaskService(IRepository<PageTask> repo) => _repo = repo;

        public async Task<IEnumerable<PageTaskResponse>> GetByChapterAsync(Guid chapterId)
            => await _repo.GetAll().Include(t => t.Assistant).Where(t => t.ChapterId == chapterId && t.DeletedAt == null).Select(t => Map(t)).ToListAsync();

        public async Task<IEnumerable<PageTaskResponse>> GetByManuscriptAsync(Guid manuscriptId)
            => await _repo.GetAll().Include(t => t.Assistant).Where(t => t.ManuscriptId == manuscriptId && t.DeletedAt == null).Select(t => Map(t)).ToListAsync();

        public async Task<IEnumerable<PageTaskResponse>> GetByAssistantAsync(Guid assistantId)
            => await _repo.GetAll().Include(t => t.Assistant).Where(t => t.AssistantId == assistantId && t.DeletedAt == null).Select(t => Map(t)).ToListAsync();

        public async Task<PageTaskResponse> GetByIdAsync(Guid id)
        {
            var t = await _repo.GetAll().Include(t => t.Assistant).FirstOrDefaultAsync(x => x.PageTaskId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("PageTask not found.");
            return Map(t);
        }

        public async Task<PageTaskResponse> CreateAsync(CreatePageTaskRequest request)
        {
            var task = new PageTask
            {
                ChapterId = request.ChapterId,
                ManuscriptId = request.ManuscriptId,
                AssistantId = request.AssistantId,
                PageStart = request.PageStart,
                PageEnd = request.PageEnd,
                TaskType = request.TaskType,
                Description = request.Description,
                DueDate = request.DueDate,
                Status = PageTaskStatus.Assigned,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(task);
            await _repo.SaveChangeAsync();
            return await GetByIdAsync(task.PageTaskId);
        }

        public async Task<PageTaskResponse> UpdateAsync(Guid id, UpdatePageTaskRequest request)
        {
            var task = await _repo.GetAll().FirstOrDefaultAsync(x => x.PageTaskId == id && x.DeletedAt == null)
                       ?? throw new KeyNotFoundException("PageTask not found.");
            if (request.Status != null && Enum.TryParse<PageTaskStatus>(request.Status, out var status)) task.Status = status;
            if (request.Description != null) task.Description = request.Description;
            if (request.DueDate.HasValue) task.DueDate = request.DueDate;
            task.UpdatedAt = DateTime.UtcNow;
            if (task.Status == PageTaskStatus.Approved) task.ApprovedAt = DateTime.UtcNow;
            _repo.Update(task);
            await _repo.SaveChangeAsync();
            return await GetByIdAsync(id);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var task = await _repo.GetAll().FirstOrDefaultAsync(x => x.PageTaskId == id && x.DeletedAt == null)
                       ?? throw new KeyNotFoundException("PageTask not found.");
            task.DeletedAt = DateTime.UtcNow;
            _repo.Update(task);
            await _repo.SaveChangeAsync();
        }

        private static PageTaskResponse Map(PageTask t) => new()
        {
            PageTaskId = t.PageTaskId, ChapterId = t.ChapterId, ManuscriptId = t.ManuscriptId,
            AssistantId = t.AssistantId, AssistantName = t.Assistant?.DisplayName ?? "",
            PageStart = t.PageStart, PageEnd = t.PageEnd, TaskType = t.TaskType,
            Description = t.Description, DueDate = t.DueDate, Status = t.Status.ToString(),
            CreatedAt = t.CreatedAt, ApprovedAt = t.ApprovedAt
        };
    }
}
