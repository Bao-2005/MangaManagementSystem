using MangaManagementSystem.Business.DTOs.Requests.Chapters;
using MangaManagementSystem.Business.DTOs.Responses.Chapters;
using MangaManagementSystem.Business.Services.Interfaces.Chapters;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Chapters
{
    public class ChapterPageService : IChapterPageService
    {
        private readonly IRepository<ChapterPage> _repo;

        public ChapterPageService(IRepository<ChapterPage> repo) => _repo = repo;

        public async Task<IEnumerable<ChapterPageResponse>> GetByChapterAsync(Guid chapterId)
            => await _repo.GetAll()
                .Where(p => p.ChapterId == chapterId && p.DeletedAt == null)
                .OrderBy(p => p.PageNo)
                .Select(p => Map(p)).ToListAsync();

        public async Task<ChapterPageResponse> GetByIdAsync(Guid id)
        {
            var p = await _repo.GetAll().FirstOrDefaultAsync(x => x.ChapterPageId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("ChapterPage not found.");
            return Map(p);
        }

        public async Task<ChapterPageResponse> CreateAsync(CreateChapterPageRequest request)
        {
            var page = new ChapterPage
            {
                ChapterId = request.ChapterId,
                PageNo = request.PageNo,
                ImageFileAssetId = request.ImageFileAssetId,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(page);
            await _repo.SaveChangeAsync();
            return Map(page);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var p = await _repo.GetAll().FirstOrDefaultAsync(x => x.ChapterPageId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("ChapterPage not found.");
            p.DeletedAt = DateTime.UtcNow;
            _repo.Update(p);
            await _repo.SaveChangeAsync();
        }

        private static ChapterPageResponse Map(ChapterPage p) => new()
        { ChapterPageId = p.ChapterPageId, ChapterId = p.ChapterId, PageNo = p.PageNo, ImageFileAssetId = p.ImageFileAssetId, CreatedAt = p.CreatedAt };
    }
}
