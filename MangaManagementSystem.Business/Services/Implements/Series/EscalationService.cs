using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class EscalationService : IEscalationService
    {
        private readonly IRepository<Escalation> _repo;

        public EscalationService(IRepository<Escalation> repo) => _repo = repo;

        public async Task<IEnumerable<EscalationResponse>> GetBySeriesAsync(Guid seriesId)
            => await _repo.GetAll().Include(e => e.Creator).Include(e => e.Resolver)
                .Where(e => e.SeriesId == seriesId && e.DeletedAt == null)
                .Select(e => Map(e)).ToListAsync();

        public async Task<EscalationResponse> GetByIdAsync(Guid id)
        {
            var e = await _repo.GetAll().Include(e => e.Creator).Include(e => e.Resolver)
                .FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Escalation not found.");
            return Map(e);
        }

        public async Task<EscalationResponse> CreateAsync(Guid createdByUserId, CreateEscalationRequest request)
        {
            var esc = new Escalation
            {
                Type = request.Type, EntityType = request.EntityType, EntityId = request.EntityId,
                SeriesId = request.SeriesId, Priority = request.Priority, Reason = request.Reason,
                Status = "Open", CreatedBy = createdByUserId, CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(esc);
            await _repo.SaveChangeAsync();
            return await GetByIdAsync(esc.EscalationId);
        }

        public async Task<EscalationResponse> ResolveAsync(Guid id, Guid resolverUserId, UpdateEscalationRequest request)
        {
            var e = await _repo.GetAll().Include(e => e.Creator).Include(e => e.Resolver)
                .FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Escalation not found.");
            if (request.Status != null) e.Status = request.Status;
            if (request.Resolution != null) e.Resolution = request.Resolution;
            e.ResolvedBy = resolverUserId;
            e.ResolvedAt = DateTime.UtcNow;
            _repo.Update(e);
            await _repo.SaveChangeAsync();
            return Map(e);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var e = await _repo.GetAll().FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Escalation not found.");
            e.DeletedAt = DateTime.UtcNow;
            _repo.Update(e);
            await _repo.SaveChangeAsync();
        }

        private static EscalationResponse Map(Escalation e) => new()
        {
            EscalationId = e.EscalationId, Type = e.Type, EntityType = e.EntityType, EntityId = e.EntityId,
            SeriesId = e.SeriesId, Priority = e.Priority, Status = e.Status, Reason = e.Reason,
            Resolution = e.Resolution, CreatedBy = e.CreatedBy, CreatorName = e.Creator?.DisplayName ?? "",
            ResolvedBy = e.ResolvedBy, ResolverName = e.Resolver?.DisplayName,
            CreatedAt = e.CreatedAt, ResolvedAt = e.ResolvedAt
        };
    }
}
