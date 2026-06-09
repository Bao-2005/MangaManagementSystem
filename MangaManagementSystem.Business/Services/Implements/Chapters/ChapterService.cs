using MangaManagementSystem.Business.DTOs.Requests.Chapters;
using MangaManagementSystem.Business.DTOs.Responses.Chapters;
using MangaManagementSystem.Business.Services.Interfaces.Chapters;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Chapters
{
    public class ChapterService : IChapterService
    {
        private readonly IRepository<Chapter> _repo;
        private readonly IRepository<Manuscript> _manuscriptRepo;
        private readonly IRepository<PageTask> _pageTaskRepo;

        public ChapterService(IRepository<Chapter> repo, IRepository<Manuscript> manuscriptRepo, IRepository<PageTask> pageTaskRepo)
        {
            _repo = repo;
            _manuscriptRepo = manuscriptRepo;
            _pageTaskRepo = pageTaskRepo;
        }

        public async Task<IEnumerable<ChapterResponse>> GetAllAsync()
            => await _repo.GetAll().Where(c => c.DeletedAt == null).Select(c => Map(c)).ToListAsync();

        public async Task<IEnumerable<ChapterResponse>> GetBySeriesAsync(Guid seriesId)
            => await _repo.GetAll().Where(c => c.SeriesId == seriesId && c.DeletedAt == null)
                .OrderBy(c => c.ChapterNo).Select(c => Map(c)).ToListAsync();

        public async Task<ChapterResponse> GetByIdAsync(Guid id)
        {
            var c = await _repo.GetAll().FirstOrDefaultAsync(x => x.ChapterId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Chapter not found.");
            return Map(c);
        }

        public async Task<ChapterResponse> CreateAsync(CreateChapterRequest request)
        {
            // BR-03: deadline = publicationDate - 14 days if not provided
            var deadline = request.SubmissionDeadline
                           ?? (request.PublicationDate.HasValue ? request.PublicationDate.Value.AddDays(-14) : null);

            var chapter = new Chapter
            {
                SeriesId = request.SeriesId,
                ChapterNo = request.ChapterNo,
                Title = request.Title,
                TotalPages = request.TotalPages,
                PublicationDate = request.PublicationDate,
                SubmissionDeadline = deadline,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(chapter);
            await _repo.SaveChangeAsync();
            return Map(chapter);
        }

        public async Task<ChapterResponse> UpdateAsync(Guid id, UpdateChapterRequest request)
        {
            var c = await _repo.GetAll().FirstOrDefaultAsync(x => x.ChapterId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Chapter not found.");
            if (request.Title != null) c.Title = request.Title;
            if (request.TotalPages.HasValue) c.TotalPages = request.TotalPages.Value;
            if (request.PublicationDate.HasValue) c.PublicationDate = request.PublicationDate;
            if (request.SubmissionDeadline.HasValue) c.SubmissionDeadline = request.SubmissionDeadline;
            if (request.Status != null) c.Status = request.Status;
            _repo.Update(c);
            await _repo.SaveChangeAsync();
            return Map(c);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var chapter = await _repo.GetAll()
                .Include(c => c.Manuscripts)
                .Include(c => c.PageTasks)
                .FirstOrDefaultAsync(x => x.ChapterId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Chapter not found.");

            var now = DateTime.UtcNow;
            chapter.DeletedAt = now;
            foreach (var m in chapter.Manuscripts.Where(m => m.DeletedAt == null)) m.DeletedAt = now;
            foreach (var pt in chapter.PageTasks.Where(pt => pt.DeletedAt == null)) pt.DeletedAt = now;
            _repo.Update(chapter);
            await _repo.SaveChangeAsync();
        }

        private static ChapterResponse Map(Chapter c) => new()
        {
            ChapterId = c.ChapterId, SeriesId = c.SeriesId, ChapterNo = c.ChapterNo, Title = c.Title,
            TotalPages = c.TotalPages, Status = c.Status, PublicationDate = c.PublicationDate,
            SubmissionDeadline = c.SubmissionDeadline, CreatedAt = c.CreatedAt
        };
    }
}
