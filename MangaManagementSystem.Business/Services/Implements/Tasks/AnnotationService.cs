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

        public AnnotationService(IRepository<Annotation> repo) => _repo = repo;

        public async Task<IEnumerable<AnnotationResponse>> GetByManuscriptAsync(Guid manuscriptId)
            => await _repo.GetAll()
                .Include(a => a.Author)
                .Include(a => a.Manuscript)
                .Where(a => a.ManuscriptId == manuscriptId && a.DeletedAt == null)
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
            var annotation = new Annotation
            {
                ManuscriptId = request.ManuscriptId,
                AuthorId = authorId,
                PageNo = request.PageNo,
                PositionX = request.PositionX,
                PositionY = request.PositionY,
                Content = request.Content,
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
            a.Content = request.Content;
            _repo.Update(a);
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

        private static AnnotationResponse Map(Annotation a) => new()
        {
            AnnotationId = a.AnnotationId, ManuscriptId = a.ManuscriptId,
            ChapterId = a.Manuscript?.ChapterId ?? Guid.Empty,
            AuthorId = a.AuthorId, AuthorName = a.Author?.DisplayName ?? "",
            PageNo = a.PageNo, PositionX = a.PositionX, PositionY = a.PositionY,
            Content = a.Content, CreatedAt = a.CreatedAt
        };
    }
}
