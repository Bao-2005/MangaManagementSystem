using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Services.Implements
{
    public class RankingSnapshotService : IRankingSnapshotService
    {
        private readonly IRepository<RankingSnapshot> _repo;

        public RankingSnapshotService(IRepository<RankingSnapshot> repo) => _repo = repo;

        public async Task<IEnumerable<RankingSnapshotResponse>> GetAllByPeriodAsync(string? period = null)
        {
            var query = _repo.GetAll().Include(r => r.Series).Where(r => r.DeletedAt == null);
            if (!string.IsNullOrWhiteSpace(period)) query = query.Where(r => r.Period == period);
            return await query.OrderBy(r => r.RankNo).Select(r => Map(r)).ToListAsync();
        }

        public async Task<IEnumerable<RankingSnapshotResponse>> GetBySeriesAsync(Guid seriesId)
            => await _repo.GetAll().Include(r => r.Series)
                .Where(r => r.SeriesId == seriesId && r.DeletedAt == null)
                .Select(r => Map(r)).ToListAsync();

        public async Task SoftDeleteAsync(Guid id)
        {
            var r = await _repo.GetAll().FirstOrDefaultAsync(x => x.RankingSnapshotId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("RankingSnapshot not found.");
            r.DeletedAt = DateTime.UtcNow;
            _repo.Update(r);
            await _repo.SaveChangeAsync();
        }

        private static RankingSnapshotResponse Map(RankingSnapshot r) => new()
        {
            RankingSnapshotId = r.RankingSnapshotId, SeriesId = r.SeriesId,
            SeriesTitle = r.Series?.Title ?? "", Period = r.Period,
            RankNo = r.RankNo, IsBottom20Percent = r.IsBottom20Percent, CreatedAt = r.CreatedAt
        };
    }
}
