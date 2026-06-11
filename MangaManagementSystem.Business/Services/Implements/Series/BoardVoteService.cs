using MangaManagement.DataAccess.DbContexts;
using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Exceptions;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class BoardVoteService : IBoardVoteService
    {
        private readonly IRepository<BoardVote> _repo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<UserAssignment> _assignmentRepo;
        private readonly IBoardDecisionFinalizationService _finalizationService;
        private readonly MangaDbContext _dbContext;

        public BoardVoteService(
            IRepository<BoardVote> repo,
            IRepository<User> userRepo,
            IRepository<UserAssignment> assignmentRepo,
            IBoardDecisionFinalizationService finalizationService,
            MangaDbContext dbContext)
        {
            _repo = repo;
            _userRepo = userRepo;
            _assignmentRepo = assignmentRepo;
            _finalizationService = finalizationService;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<BoardVoteResponse>> GetByDecisionAsync(Guid boardDecisionId)
            => await _repo.GetAll().Include(v => v.Voter)
                .Where(v => v.BoardDecisionId == boardDecisionId && v.DeletedAt == null)
                .Select(v => Map(v)).ToListAsync();

        public async Task<BoardDecisionSummaryResponse> CastVoteAsync(Guid voterId, Guid boardDecisionId, CreateBoardVoteRequest request)
        {
            if (!request.VoteValue.HasValue)
            {
                throw new ArgumentException("VoteValue is required.");
            }

            var voteValue = request.VoteValue.Value;
            var comment = request.Comment?.Trim();
            if (!voteValue && (string.IsNullOrWhiteSpace(comment) || comment.Length < 50))
            {
                throw new ArgumentException("Reject votes require a comment with at least 50 characters.");
            }

            await EnsureActiveEditorialBoardAsync(voterId);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var decision = await GetOpenDecisionForVotingAsync(boardDecisionId);
            await EnsureNoConflictOfInterestAsync(voterId, decision);

            // BR-01: prevent duplicate votes (unique index in DB handles it, but guard here too)
            var already = await _repo.GetAll()
                .AnyAsync(v => v.BoardDecisionId == boardDecisionId && v.VoterId == voterId && v.DeletedAt == null);
            if (already) throw new InvalidOperationException("You have already voted on this decision.");

            var vote = new BoardVote
            {
                BoardDecisionId = boardDecisionId,
                VoterId = voterId,
                VoteValue = voteValue,
                Comment = comment,
                VotedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(vote);

            try
            {
                await _repo.SaveChangeAsync();
            }
            catch (DbUpdateException ex) when (IsDuplicateVoteConflict(ex))
            {
                throw new InvalidOperationException("You have already voted on this decision.", ex);
            }

            await _finalizationService.RecalculateAsync(boardDecisionId);

            var summary = await _finalizationService.GetSummaryAsync(boardDecisionId);
            await transaction.CommitAsync();
            return summary;
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

        private async Task EnsureActiveEditorialBoardAsync(Guid voterId)
        {
            var isActiveBoardMember = await _userRepo.GetAll()
                .Include(u => u.Role)
                .AnyAsync(u => u.UserId == voterId
                    && u.DeletedAt == null
                    && u.Role.DeletedAt == null
                    && u.Role.RoleName == UserRole.EditorialBoard.ToString());

            if (!isActiveBoardMember)
            {
                throw new ForbiddenAccessException("Only active EditorialBoard users can vote on board decisions.");
            }
        }

        private async Task EnsureNoConflictOfInterestAsync(Guid voterId, BoardDecision decision)
        {
            if (decision.Series.MangakaId == voterId || decision.CreatedBy == voterId)
            {
                throw new ForbiddenAccessException("You cannot vote on a decision where you have a conflict of interest.");
            }

            var conflictingRoles = new[]
            {
                UserRole.TantouEditor.ToString(),
                UserRole.Assistant.ToString()
            };

            var hasAssignmentConflict = await _assignmentRepo.GetAll()
                .Include(a => a.FromUser).ThenInclude(u => u.Role)
                .AnyAsync(a => a.ToUserId == decision.Series.MangakaId
                    && a.FromUserId == voterId
                    && a.UnassignedAt == null
                    && a.DeletedAt == null
                    && conflictingRoles.Contains(a.FromUser.Role.RoleName));

            if (hasAssignmentConflict)
            {
                throw new ForbiddenAccessException("You cannot vote on a decision where you have a conflict of interest.");
            }
        }

        private async Task<BoardDecision> GetOpenDecisionForVotingAsync(Guid boardDecisionId)
        {
            var decision = await _dbContext.BoardDecisions
                .FromSqlInterpolated($"SELECT * FROM \"BoardDecisions\" WHERE \"BoardDecisionId\" = {boardDecisionId} AND \"DeletedAt\" IS NULL FOR UPDATE")
                .Include(d => d.Series)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("BoardDecision not found.");

            if (decision.FinalizedAt.HasValue || !string.Equals(decision.Status, "Open", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Voting is closed for this decision.");
            }

            if (DateTime.UtcNow > decision.VotingDeadline)
            {
                throw new InvalidOperationException("Voting deadline has passed.");
            }

            return decision;
        }

        private static bool IsDuplicateVoteConflict(DbUpdateException exception)
        {
            return exception.InnerException is PostgresException postgresException
                && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
                && string.Equals(
                    postgresException.ConstraintName,
                    "IX_BoardVotes_BoardDecisionId_VoterId",
                    StringComparison.Ordinal);
        }
    }
}
