using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class BoardDecisionFinalizationService : IBoardDecisionFinalizationService
    {
        private const int Quorum = 3;
        private const string OpenStatus = "Open";
        private const string FinalizedStatus = "Finalized";
        private const string ApprovedResult = "Approved";
        private const string RejectedResult = "Rejected";

        private readonly IRepository<BoardDecision> _decisionRepo;
        private readonly IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> _seriesRepo;
        private readonly IRepository<UserAssignment> _assignmentRepo;

        public BoardDecisionFinalizationService(
            IRepository<BoardDecision> decisionRepo,
            IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> seriesRepo,
            IRepository<UserAssignment> assignmentRepo)
        {
            _decisionRepo = decisionRepo;
            _seriesRepo = seriesRepo;
            _assignmentRepo = assignmentRepo;
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

            var approveCount = validVotes.Count(v => v.VoteValue);
            var rejectCount = validVotes.Count - approveCount;
            var result = approveCount > validVotes.Count / 2.0
                ? ApprovedResult
                : rejectCount > validVotes.Count / 2.0
                    ? RejectedResult
                    : null;

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

        public async Task<BoardDecisionSummaryResponse> GetSummaryAsync(Guid boardDecisionId)
        {
            var decision = await GetDecisionWithVotesAsync(boardDecisionId);
            var validVotes = await GetValidVotesAsync(decision);

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
                RejectCount = validVotes.Count(v => !v.VoteValue)
            };
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

        private async Task<List<BoardVote>> GetValidVotesAsync(BoardDecision decision)
        {
            var assignedConflictUserIds = await _assignmentRepo.GetAll()
                .Where(a => a.FromUserId == decision.Series.MangakaId
                    && a.Status
                    && a.UnassignedAt == null
                    && a.DeletedAt == null
                    && (a.AssignmentType == AssignmentType.TantouEditor.ToString()
                        || a.AssignmentType == AssignmentType.Assistant.ToString()))
                .Select(a => a.ToUserId)
                .ToListAsync();

            return decision.BoardVotes
                .Where(v => v.DeletedAt == null
                    && v.Voter.DeletedAt == null
                    && v.Voter.Role.DeletedAt == null
                    && v.Voter.Role.RoleName == UserRole.EditorialBoard.ToString()
                    && v.VoterId != decision.Series.MangakaId
                    && v.VoterId != decision.CreatedBy
                    && !assignedConflictUserIds.Contains(v.VoterId))
                .ToList();
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
