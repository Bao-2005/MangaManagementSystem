using MangaManagement.DataAccess.DbContexts;
using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class BoardDecisionFinalizationService : IBoardDecisionFinalizationService
    {
        private const int Quorum = 3;
        private const string OpenStatus = "Open";
        private const string FinalizedStatus = "Finalized";
        private const string TieStatus = "Tie";
        private const string ApprovedResult = "Approved";
        private const string RejectedResult = "Rejected";
        private const string NoQuorumResult = "NoQuorum";
        private const string TieResult = "Tie";

        private readonly IRepository<BoardDecision> _decisionRepo;
        private readonly IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> _seriesRepo;
        private readonly IRepository<UserAssignment> _assignmentRepo;
        private readonly INotificationDispatchService _notificationDispatchService;
        private readonly MangaDbContext _dbContext;
        private readonly ILogger<BoardDecisionFinalizationService> _logger;

        public BoardDecisionFinalizationService(
            IRepository<BoardDecision> decisionRepo,
            IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> seriesRepo,
            IRepository<UserAssignment> assignmentRepo,
            INotificationDispatchService notificationDispatchService,
            MangaDbContext dbContext,
            ILogger<BoardDecisionFinalizationService> logger)
        {
            _decisionRepo = decisionRepo;
            _seriesRepo = seriesRepo;
            _assignmentRepo = assignmentRepo;
            _notificationDispatchService = notificationDispatchService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task RecalculateAsync(Guid boardDecisionId)
        {
            var decision = await GetDecisionWithVotesAsync(boardDecisionId);

            if (decision.FinalizedAt.HasValue || !string.Equals(decision.Status, OpenStatus, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var validVotes = await GetValidVotesAsync(decision);
            if (validVotes.Count < Quorum)
            {
                return;
            }

            var result = GetMajorityResult(validVotes);

            if (result == null)
            {
                return;
            }

            var now = DateTime.UtcNow;
            decision.Status = FinalizedStatus;
            decision.Result = result;
            decision.FinalizedAt = now;

            decision.Series.Status = result == ApprovedResult
                ? SeriesStatus.Approved
                : SeriesStatus.Rejected;

            if (result == RejectedResult)
            {
                decision.Series.RejectReason = BuildRejectReason(validVotes);
            }

            _decisionRepo.Update(decision);
            _seriesRepo.Update(decision.Series);
            await _decisionRepo.SaveChangeAsync();
        }

        public async Task<BoardDecisionSummaryResponse> ProcessDeadlineAsync(Guid boardDecisionId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var decision = await GetDecisionWithVotesForUpdateAsync(boardDecisionId);

            if (decision.FinalizedAt.HasValue || !string.Equals(decision.Status, OpenStatus, StringComparison.OrdinalIgnoreCase))
            {
                var existingSummary = await BuildSummaryAsync(decision);
                await transaction.CommitAsync();
                return existingSummary;
            }

            if (DateTime.UtcNow <= decision.VotingDeadline)
            {
                var openSummary = await BuildSummaryAsync(decision);
                await transaction.CommitAsync();
                return openSummary;
            }

            var validVotes = await GetValidVotesAsync(decision);
            var now = DateTime.UtcNow;

            if (validVotes.Count < Quorum)
            {
                decision.Status = FinalizedStatus;
                decision.Result = NoQuorumResult;
                decision.FinalizedAt = now;
                decision.Series.Status = SeriesStatus.Expired;

                _decisionRepo.Update(decision);
                _seriesRepo.Update(decision.Series);
                await _decisionRepo.SaveChangeAsync();

                var summary = CreateSummary(decision, validVotes);
                await transaction.CommitAsync();
                await TryNotifyEditorInChiefOfFailedOutcomeAsync(decision);
                return summary;
            }

            var result = GetMajorityResult(validVotes);

            if (result != null)
            {
                decision.Status = FinalizedStatus;
                decision.Result = result;
                decision.FinalizedAt = now;
                decision.Series.Status = result == ApprovedResult
                    ? SeriesStatus.Approved
                    : SeriesStatus.Rejected;

                if (result == RejectedResult)
                {
                    decision.Series.RejectReason = BuildRejectReason(validVotes);
                }

                _decisionRepo.Update(decision);
                _seriesRepo.Update(decision.Series);
                await _decisionRepo.SaveChangeAsync();

                var summary = CreateSummary(decision, validVotes);
                await transaction.CommitAsync();
                return summary;
            }

            if (validVotes.Count(v => v.VoteValue) == validVotes.Count(v => !v.VoteValue))
            {
                decision.Status = TieStatus;
                decision.Result = TieResult;
                decision.FinalizedAt = now;

                _decisionRepo.Update(decision);
                await _decisionRepo.SaveChangeAsync();

                var summary = CreateSummary(decision, validVotes);
                await transaction.CommitAsync();
                await TryNotifyEditorInChiefOfFailedOutcomeAsync(decision);
                return summary;
            }

            var unchangedSummary = await BuildSummaryAsync(decision);
            await transaction.CommitAsync();
            return unchangedSummary;
        }

        public async Task<BoardDecisionSummaryResponse> GetSummaryAsync(Guid boardDecisionId)
        {
            var decision = await GetDecisionWithVotesAsync(boardDecisionId);
            return await BuildSummaryAsync(decision);
        }

        private async Task<BoardDecision> GetDecisionWithVotesAsync(Guid boardDecisionId)
        {
            return await _decisionRepo.GetAll()
                .Include(d => d.Series)
                .Include(d => d.BoardVotes)
                    .ThenInclude(v => v.Voter)
                    .ThenInclude(v => v.Role)
                .FirstOrDefaultAsync(d => d.BoardDecisionId == boardDecisionId && d.DeletedAt == null)
                ?? throw new KeyNotFoundException("BoardDecision not found.");
        }

        private async Task<BoardDecision> GetDecisionWithVotesForUpdateAsync(Guid boardDecisionId)
        {
            return await _dbContext.BoardDecisions
                .FromSqlInterpolated($"SELECT * FROM \"BoardDecisions\" WHERE \"BoardDecisionId\" = {boardDecisionId} AND \"DeletedAt\" IS NULL FOR UPDATE")
                .Include(d => d.Series)
                .Include(d => d.BoardVotes)
                    .ThenInclude(v => v.Voter)
                    .ThenInclude(v => v.Role)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("BoardDecision not found.");
        }

        private async Task<List<BoardVote>> GetValidVotesAsync(BoardDecision decision)
        {
            var conflictingRoles = new[]
            {
                UserRole.TantouEditor.ToString(),
                UserRole.Assistant.ToString()
            };

            var assignedConflictUserIds = await _assignmentRepo.GetAll()
                .Include(a => a.FromUser).ThenInclude(u => u.Role)
                .Where(a => a.ToUserId == decision.Series.MangakaId
                    && a.UnassignedAt == null
                    && a.DeletedAt == null
                    && conflictingRoles.Contains(a.FromUser.Role.RoleName))
                .Select(a => a.FromUserId)
                .ToListAsync();

            return decision.BoardVotes
                .Where(v => v.DeletedAt == null
                    && v.Voter.DeletedAt == null
                    && v.Voter.Role != null
                    && v.Voter.Role.DeletedAt == null
                    && v.Voter.Role.RoleName == UserRole.EditorialBoard.ToString()
                    && v.VoterId != decision.Series.MangakaId
                    && v.VoterId != decision.CreatedBy
                    && !assignedConflictUserIds.Contains(v.VoterId))
                .ToList();
        }

        private static string? GetMajorityResult(List<BoardVote> validVotes)
        {
            var approveCount = validVotes.Count(v => v.VoteValue);
            var rejectCount = validVotes.Count - approveCount;

            return approveCount > validVotes.Count / 2.0
                ? ApprovedResult
                : rejectCount > validVotes.Count / 2.0
                    ? RejectedResult
                    : null;
        }

        private async Task<BoardDecisionSummaryResponse> BuildSummaryAsync(BoardDecision decision)
        {
            var validVotes = await GetValidVotesAsync(decision);
            return CreateSummary(decision, validVotes);
        }

        private static BoardDecisionSummaryResponse CreateSummary(BoardDecision decision, List<BoardVote> validVotes)
        {
            return new BoardDecisionSummaryResponse
            {
                BoardDecisionId = decision.BoardDecisionId,
                SeriesId = decision.SeriesId,
                DecisionType = decision.DecisionType,
                Status = decision.Status,
                Result = decision.Result,
                VotingDeadline = decision.VotingDeadline,
                FinalizedAt = decision.FinalizedAt,
                VoteCount = validVotes.Count,
                ApproveCount = validVotes.Count(v => v.VoteValue),
                RejectCount = validVotes.Count(v => !v.VoteValue),
                RequiredQuorum = Quorum,
                HasQuorum = validVotes.Count >= Quorum,
                IsDeadlinePassed = DateTime.UtcNow > decision.VotingDeadline
            };
        }

        private async Task<NotificationDispatchResponse> NotifyEditorInChiefOfFailedOutcomeAsync(BoardDecision decision)
        {
            var outcome = string.Equals(decision.Result, NoQuorumResult, StringComparison.OrdinalIgnoreCase)
                ? "no quorum"
                : "a tie";
            var requiresSpecialDecision = decision.ExtensionCount >= 1;

            return await _notificationDispatchService.DispatchToRoleAsync(
                new NotificationDispatchRequest
                {
                    Title = requiresSpecialDecision
                        ? "Board vote requires special decision"
                        : "Board vote requires deadline extension review",
                    Message = requiresSpecialDecision
                        ? $"Extended board decision for series '{decision.Series.Title}' ended in {outcome} and requires a special decision."
                        : $"Board decision for series '{decision.Series.Title}' ended in {outcome} and requires deadline extension review.",
                    Type = requiresSpecialDecision
                        ? "BoardDecisionSpecialDecisionRequired"
                        : string.Equals(decision.Result, NoQuorumResult, StringComparison.OrdinalIgnoreCase)
                            ? "BoardDecisionNoQuorum"
                            : "BoardDecisionTie",
                    Link = $"/board-decisions/{decision.BoardDecisionId}",
                    Priority = "High"
                },
                UserRole.EditorInChief.ToString());
        }

        private async Task TryNotifyEditorInChiefOfFailedOutcomeAsync(BoardDecision decision)
        {
            try
            {
                var dispatchResult = await NotifyEditorInChiefOfFailedOutcomeAsync(decision);
                if (dispatchResult.Status == NotificationDispatchStatus.NoRecipients)
                {
                    _logger.LogWarning(
                        "Board decision {BoardDecisionId} finalized as {Result}, but Editor-in-Chief notification had no recipients: {Message}",
                        decision.BoardDecisionId,
                        decision.Result,
                        dispatchResult.Message);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Board decision {BoardDecisionId} finalized as {Result}, but Editor-in-Chief notification dispatch failed.",
                    decision.BoardDecisionId,
                    decision.Result);
            }
        }

        private static string BuildRejectReason(IEnumerable<BoardVote> validVotes)
        {
            var rejectComments = validVotes
                .Where(v => !v.VoteValue && !string.IsNullOrWhiteSpace(v.Comment))
                .Select(v => v.Comment!.Trim());

            var reason = string.Join("\n", rejectComments);
            if (string.IsNullOrWhiteSpace(reason))
            {
                reason = "Rejected by editorial board majority vote.";
            }

            return reason.Length <= 1000 ? reason : reason[..1000];
        }
    }
}
