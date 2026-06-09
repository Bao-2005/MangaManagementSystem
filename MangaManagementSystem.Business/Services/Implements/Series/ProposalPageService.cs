using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class ProposalPageService : IProposalPageService
    {
        private readonly IRepository<ProposalPage> _repo;

        public ProposalPageService(IRepository<ProposalPage> repo) => _repo = repo;

        public async Task<IEnumerable<ProposalPageResponse>> GetBySeriesAsync(Guid seriesId)
            => await _repo.GetAll()
                .Where(p => p.SeriesId == seriesId && p.DeletedAt == null)
                .OrderBy(p => p.PageNo)
                .Select(p => Map(p)).ToListAsync();

        public async Task<ProposalPageResponse> GetByIdAsync(Guid id)
        {
            var p = await _repo.GetAll().FirstOrDefaultAsync(x => x.ProposalPageId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("ProposalPage not found.");
            return Map(p);
        }

        public async Task<ProposalPageResponse> CreateAsync(Guid seriesId, CreateProposalPageRequest request)
        {
            var page = new ProposalPage
            {
                SeriesId = seriesId,
                PageNo = request.PageNo,
                PreviewFileAssetId = request.PreviewFileAssetId,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(page);
            await _repo.SaveChangeAsync();
            return Map(page);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var p = await _repo.GetAll().FirstOrDefaultAsync(x => x.ProposalPageId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("ProposalPage not found.");
            p.DeletedAt = DateTime.UtcNow;
            _repo.Update(p);
            await _repo.SaveChangeAsync();
        }

        private static ProposalPageResponse Map(ProposalPage p) => new()
        { ProposalPageId = p.ProposalPageId, SeriesId = p.SeriesId, PageNo = p.PageNo, PreviewFileAssetId = p.PreviewFileAssetId, CreatedAt = p.CreatedAt };
    }
}
