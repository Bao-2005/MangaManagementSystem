using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class BoardDecisionService : IBoardDecisionService
    {
        private readonly IRepository<BoardDecision> _repo;

        public BoardDecisionService(IRepository<BoardDecision> repo) => _repo = repo;

        public async Task<IEnumerable<BoardDecisionResponse>> GetBySeriesAsync(Guid seriesId)
            => await _repo.GetAll().Include(b => b.BoardVotes)
                .Where(b => b.SeriesId == seriesId && b.DeletedAt == null)
                .Select(b => Map(b)).ToListAsync();

        public async Task<BoardDecisionResponse> GetByIdAsync(Guid id)
        {
            var b = await _repo.GetAll().Include(b => b.BoardVotes)
                .FirstOrDefaultAsync(x => x.BoardDecisionId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("BoardDecision not found.");
            return Map(b);
        }

        public async Task<BoardDecisionResponse> CreateAsync(CreateBoardDecisionRequest request)
        {
            var decision = new BoardDecision
            {
                SeriesId = request.SeriesId,
                DecisionType = request.DecisionType,
                VotingDeadline = request.VotingDeadline,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(decision);
            await _repo.SaveChangeAsync();
            return Map(decision);
        }

        public async Task<BoardDecisionResponse> UpdateAsync(Guid id, UpdateBoardDecisionRequest request)
        {
            var b = await _repo.GetAll().Include(b => b.BoardVotes)
                .FirstOrDefaultAsync(x => x.BoardDecisionId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("BoardDecision not found.");
            if (request.Status != null) b.Status = request.Status;
            if (request.Result != null) b.Result = request.Result;
            if (request.FinalizedAt.HasValue) b.FinalizedAt = request.FinalizedAt;
            _repo.Update(b);
            await _repo.SaveChangeAsync();
            return Map(b);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var b = await _repo.GetAll().FirstOrDefaultAsync(x => x.BoardDecisionId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("BoardDecision not found.");
            b.DeletedAt = DateTime.UtcNow;
            _repo.Update(b);
            await _repo.SaveChangeAsync();
        }

        private static BoardDecisionResponse Map(BoardDecision b) => new()
        {
            BoardDecisionId = b.BoardDecisionId, SeriesId = b.SeriesId, DecisionType = b.DecisionType,
            Status = b.Status, Result = b.Result, VotingDeadline = b.VotingDeadline,
            FinalizedAt = b.FinalizedAt, CreatedBy = b.CreatedBy, CreatedAt = b.CreatedAt,
            VoteCount = b.BoardVotes?.Count(v => v.DeletedAt == null) ?? 0
        };
    }
}
