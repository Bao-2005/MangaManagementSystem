using AutoMapper;
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
        private readonly IMapper _mapper;

        public EscalationService(IRepository<Escalation> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EscalationResponse>> GetBySeriesAsync(Guid seriesId)
        {
            var entities = await _repo.GetAll()
                .Include(e => e.Creator)
                .Include(e => e.Resolver)
                .Where(e => e.SeriesId == seriesId && e.DeletedAt == null)
                .ToListAsync();
            return _mapper.Map<IEnumerable<EscalationResponse>>(entities);
        }

        public async Task<EscalationResponse> GetByIdAsync(Guid id)
        {
            var e = await _repo.GetAll()
                .Include(e => e.Creator)
                .Include(e => e.Resolver)
                .FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Escalation not found.");
            return _mapper.Map<EscalationResponse>(e);
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
            var e = await _repo.GetAll()
                .Include(e => e.Creator)
                .Include(e => e.Resolver)
                .FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Escalation not found.");
            if (request.Status != null) e.Status = request.Status;
            if (request.Resolution != null) e.Resolution = request.Resolution;
            e.ResolvedBy = resolverUserId;
            e.ResolvedAt = DateTime.UtcNow;
            _repo.Update(e);
            await _repo.SaveChangeAsync();
            return _mapper.Map<EscalationResponse>(e);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var e = await _repo.GetAll()
                .FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Escalation not found.");
            e.DeletedAt = DateTime.UtcNow;
            _repo.Update(e);
            await _repo.SaveChangeAsync();
        }
    }
}
