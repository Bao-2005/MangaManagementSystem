using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements
{
    public class BoardVoteService : IBoardVoteService
    {
        private readonly IRepository<BoardVote> _repo;
        private readonly IRepository<BoardDecision> _decisionRepo;

        public BoardVoteService(IRepository<BoardVote> repo, IRepository<BoardDecision> decisionRepo)
        {
            _repo = repo;
            _decisionRepo = decisionRepo;
        }

        public async Task<IEnumerable<BoardVoteResponse>> GetByDecisionAsync(Guid boardDecisionId)
            => await _repo.GetAll().Include(v => v.Voter)
                .Where(v => v.BoardDecisionId == boardDecisionId && v.DeletedAt == null)
                .Select(v => Map(v)).ToListAsync();

        public async Task<BoardVoteResponse> CastVoteAsync(Guid voterId, CreateBoardVoteRequest request)
        {
            // BR-01: prevent duplicate votes (unique index in DB handles it, but guard here too)
            var already = await _repo.GetAll()
                .AnyAsync(v => v.BoardDecisionId == request.BoardDecisionId && v.VoterId == voterId && v.DeletedAt == null);
            if (already) throw new InvalidOperationException("You have already voted on this decision.");

            var vote = new BoardVote
            {
                BoardDecisionId = request.BoardDecisionId,
                VoterId = voterId,
                VoteValue = request.VoteValue,
                Comment = request.Comment,
                VotedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(vote);
            await _repo.SaveChangeAsync();
            return await _repo.GetAll().Include(v => v.Voter)
                .Where(v => v.BoardVoteId == vote.BoardVoteId)
                .Select(v => Map(v)).FirstAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var v = await _repo.GetAll().FirstOrDefaultAsync(x => x.BoardVoteId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("BoardVote not found.");
            v.DeletedAt = DateTime.UtcNow;
            _repo.Update(v);
            await _repo.SaveChangeAsync();
        }

        private static BoardVoteResponse Map(BoardVote v) => new()
        {
            BoardVoteId = v.BoardVoteId, BoardDecisionId = v.BoardDecisionId, VoterId = v.VoterId,
            VoterName = v.Voter?.DisplayName ?? "", VoteValue = v.VoteValue, VotedAt = v.VotedAt, Comment = v.Comment
        };
    }
}
