using MangaManagement.DataAccess.DbContexts;
using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class BoardDecisionService : IBoardDecisionService
    {
        private const string SeriesProposalDecisionType = "SeriesProposal";
        private const string OpenStatus = "Open";
        private const string FinalizedStatus = "Finalized";
        private const string TieStatus = "Tie";
        private const string ApprovedResult = "Approved";
        private const string RejectedResult = "Rejected";
        private const string NoQuorumResult = "NoQuorum";
        private const string TieResult = "Tie";

        private readonly IRepository<BoardDecision> _repo;
        private readonly IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> _seriesRepo;
        private readonly IBoardDecisionFinalizationService _finalizationService;
        private readonly MangaDbContext _dbContext;

        public BoardDecisionService(
            IRepository<BoardDecision> repo,
            IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> seriesRepo,
            IBoardDecisionFinalizationService finalizationService,
            MangaDbContext dbContext)
        {
            _repo = repo;
            _seriesRepo = seriesRepo;
            _finalizationService = finalizationService;
            _dbContext = dbContext;
        }

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

        public async Task<BoardDecisionResponse> CreateAsync(CreateBoardDecisionRequest request, Guid? createdBy = null)
        {
            var decision = new BoardDecision
            {
                SeriesId = request.SeriesId,
                DecisionType = request.DecisionType,
                VotingDeadline = request.VotingDeadline,
                Status = "Open",
                CreatedBy = createdBy,
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

            if (request.Status != null || request.Result != null || request.FinalizedAt.HasValue)
            {
                throw new ArgumentException("Board decision finalization fields can only be changed by the board decision workflow.");
            }

            return Map(b);
        }

        public async Task<BoardDecisionResponse> ExtendDeadlineAsync(Guid id, Guid editorInChiefId, ExtendBoardDecisionRequest request)
        {
            var reason = request.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Extension reason is required.");
            }

            var newDeadline = NormalizeUtc(request.NewDeadline);
            var now = DateTime.UtcNow;
            if (newDeadline <= now)
            {
                throw new ArgumentException("NewDeadline must be in the future.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var decision = await GetDecisionForUpdateAsync(id);

            if (!string.Equals(decision.DecisionType, SeriesProposalDecisionType, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only series proposal board decisions can be extended.");
            }

            if (decision.ExtensionCount >= 1)
            {
                throw new InvalidOperationException("Board decision deadline can only be extended once.");
            }

            if (!IsFailedOutcomeForChiefReview(decision))
            {
                throw new InvalidOperationException("Only tied or no-quorum decisions can be extended.");
            }

            // Extension continues the same voting period; existing votes stay valid and voters do not recast.
            decision.ExtensionCount += 1;
            decision.ExtendedBy = editorInChiefId;
            decision.ExtendedAt = now;
            decision.ExtensionReason = reason;
            decision.VotingDeadline = newDeadline;
            decision.Status = OpenStatus;
            decision.Result = null;
            decision.FinalizedAt = null;
            decision.Series.Status = SeriesStatus.BoardVoting;

            _repo.Update(decision);
            _seriesRepo.Update(decision.Series);
            await _repo.SaveChangeAsync();
            await transaction.CommitAsync();

            return Map(decision);
        }

        public async Task<BoardDecisionResponse> SpecialDecisionAsync(Guid id, Guid editorInChiefId, SpecialBoardDecisionRequest request)
        {
            var decisionResult = NormalizeSpecialDecision(request.Decision);
            var reason = request.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Special decision reason is required.");
            }

            var currentDecision = await GetDecisionForSpecialDecisionAsync(id);
            if (string.Equals(currentDecision.Status, OpenStatus, StringComparison.OrdinalIgnoreCase)
                && DateTime.UtcNow > currentDecision.VotingDeadline)
            {
                await _finalizationService.ProcessDeadlineAsync(id);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var decision = await GetDecisionForUpdateAsync(id);

            if (decision.ExtensionCount < 1)
            {
                throw new InvalidOperationException("Special decision is only available after an extension.");
            }

            if (decision.SpecialDecisionAt.HasValue)
            {
                throw new InvalidOperationException("Special decision has already been recorded.");
            }

            if (string.Equals(decision.Status, OpenStatus, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(DateTime.UtcNow <= decision.VotingDeadline
                    ? "Special decision is only available after the extended deadline has passed."
                    : "Special decision is only available after the extended deadline is finalized.");
            }

            if (!IsFailedOutcomeForChiefReview(decision))
            {
                throw new InvalidOperationException("Special decision is only available for tied or no-quorum decisions after extension.");
            }

            var now = DateTime.UtcNow;
            decision.Status = FinalizedStatus;
            decision.Result = decisionResult;
            decision.FinalizedAt = now;
            decision.SpecialDecisionBy = editorInChiefId;
            decision.SpecialDecisionAt = now;
            decision.SpecialDecisionReason = reason;

            decision.Series.Status = decisionResult == ApprovedResult
                ? SeriesStatus.Approved
                : SeriesStatus.Rejected;

            if (decisionResult == RejectedResult)
            {
                decision.Series.RejectReason = reason;
            }

            _repo.Update(decision);
            _seriesRepo.Update(decision.Series);
            await _repo.SaveChangeAsync();
            await transaction.CommitAsync();

            return Map(decision);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var b = await _repo.GetAll().FirstOrDefaultAsync(x => x.BoardDecisionId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("BoardDecision not found.");
            b.DeletedAt = DateTime.UtcNow;
            _repo.Update(b);
            await _repo.SaveChangeAsync();
        }

        private async Task<BoardDecision> GetDecisionForSpecialDecisionAsync(Guid id)
        {
            return await _repo.GetAll()
                .Include(b => b.Series)
                .Include(b => b.BoardVotes)
                .FirstOrDefaultAsync(x => x.BoardDecisionId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("BoardDecision not found.");
        }

        private async Task<BoardDecision> GetDecisionForUpdateAsync(Guid id)
        {
            return await _dbContext.BoardDecisions
                .FromSqlInterpolated($"SELECT * FROM \"BoardDecisions\" WHERE \"BoardDecisionId\" = {id} AND \"DeletedAt\" IS NULL FOR UPDATE")
                .Include(b => b.Series)
                .Include(b => b.BoardVotes)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("BoardDecision not found.");
        }

        private static bool IsFailedOutcomeForChiefReview(BoardDecision decision)
        {
            return string.Equals(decision.Result, NoQuorumResult, StringComparison.OrdinalIgnoreCase)
                || string.Equals(decision.Result, TieResult, StringComparison.OrdinalIgnoreCase)
                || string.Equals(decision.Status, TieStatus, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeSpecialDecision(string? decision)
        {
            var normalized = decision?.Trim();
            if (string.Equals(normalized, ApprovedResult, StringComparison.OrdinalIgnoreCase))
            {
                return ApprovedResult;
            }

            if (string.Equals(normalized, RejectedResult, StringComparison.OrdinalIgnoreCase))
            {
                return RejectedResult;
            }

            throw new ArgumentException("Special decision must be Approved or Rejected.");
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }

        private static BoardDecisionResponse Map(BoardDecision b) => new()
        {
            BoardDecisionId = b.BoardDecisionId, SeriesId = b.SeriesId, DecisionType = b.DecisionType,
            Status = b.Status, Result = b.Result, VotingDeadline = b.VotingDeadline,
            FinalizedAt = b.FinalizedAt, CreatedBy = b.CreatedBy, CreatedAt = b.CreatedAt,
            ExtensionCount = b.ExtensionCount, ExtendedBy = b.ExtendedBy, ExtendedAt = b.ExtendedAt,
            ExtensionReason = b.ExtensionReason, SpecialDecisionBy = b.SpecialDecisionBy,
            SpecialDecisionAt = b.SpecialDecisionAt, SpecialDecisionReason = b.SpecialDecisionReason,
            VoteCount = b.BoardVotes?.Count(v => v.DeletedAt == null) ?? 0
        };
    }
}
