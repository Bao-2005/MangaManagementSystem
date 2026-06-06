using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements
{
    public class VoteRecordService : IVoteRecordService
    {
        private readonly IRepository<VoteRecord> _repo;

        public VoteRecordService(IRepository<VoteRecord> repo) => _repo = repo;

        public async Task<IEnumerable<VoteRecordResponse>> GetBySeriesAsync(Guid seriesId)
            => await _repo.GetAll().Include(v => v.Confirmer)
                .Where(v => v.SeriesId == seriesId && v.DeletedAt == null)
                .Select(v => Map(v)).ToListAsync();

        public async Task<IEnumerable<VoteRecordResponse>> GetByPeriodAsync(string period)
            => await _repo.GetAll().Include(v => v.Confirmer)
                .Where(v => v.Period == period && v.DeletedAt == null)
                .Select(v => Map(v)).ToListAsync();

        public async Task<VoteRecordResponse> CreateAsync(CreateVoteRecordRequest request)
        {
            var vr = new VoteRecord
            {
                SeriesId = request.SeriesId,
                Period = request.Period,
                ReaderCount = request.ReaderCount,
                VoteCount = request.VoteCount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(vr);
            await _repo.SaveChangeAsync();
            return Map(vr);
        }

        public async Task<VoteRecordResponse> ConfirmAsync(Guid id, Guid confirmerId)
        {
            var vr = await _repo.GetAll().Include(v => v.Confirmer)
                .FirstOrDefaultAsync(x => x.VoteRecordId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("VoteRecord not found.");
            vr.Status = "Confirmed";
            vr.ConfirmedBy = confirmerId;
            vr.ConfirmedAt = DateTime.UtcNow;
            _repo.Update(vr);
            await _repo.SaveChangeAsync();
            return Map(vr);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var vr = await _repo.GetAll().FirstOrDefaultAsync(x => x.VoteRecordId == id && x.DeletedAt == null)
                     ?? throw new KeyNotFoundException("VoteRecord not found.");
            vr.DeletedAt = DateTime.UtcNow;
            _repo.Update(vr);
            await _repo.SaveChangeAsync();
        }

        private static VoteRecordResponse Map(VoteRecord v) => new()
        {
            VoteRecordId = v.VoteRecordId, SeriesId = v.SeriesId, Period = v.Period,
            ReaderCount = v.ReaderCount, VoteCount = v.VoteCount, Status = v.Status,
            ConfirmedBy = v.ConfirmedBy, ConfirmerName = v.Confirmer?.DisplayName,
            ConfirmedAt = v.ConfirmedAt, CreatedAt = v.CreatedAt
        };
    }
}
