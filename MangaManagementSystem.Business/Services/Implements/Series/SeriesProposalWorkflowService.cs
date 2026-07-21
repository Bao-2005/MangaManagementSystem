using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Exceptions;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class SeriesProposalWorkflowService : ISeriesProposalWorkflowService
    {
        // private const int MinimumProposalPageCount = 5; // Rule disabled: minimum 5 pages not required

        private const string SeriesProposalDecisionType = "SeriesProposal";
        private const string OpenDecisionStatus = "Open";
        private const string OpenSeriesProposalDecisionIndexName = "IX_BoardDecisions_OpenSeriesProposal_SeriesId";

        private static readonly string[] ValidPublicationTypes =
        {
            "Weekly",
            "Monthly",
            "One-shot"
        };

        private readonly IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> _seriesRepo;
        private readonly IRepository<ProposalPage> _proposalPageRepo;
        private readonly IRepository<UserAssignment> _userAssignmentRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<BoardDecision> _boardDecisionRepo;
        private readonly ISeriesService _seriesService;
        private readonly INotificationDispatchService _notificationDispatchService;
        private readonly ILogger<SeriesProposalWorkflowService> _logger;

        public SeriesProposalWorkflowService(
            IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> seriesRepo,
            IRepository<ProposalPage> proposalPageRepo,
            IRepository<UserAssignment> userAssignmentRepo,
            IRepository<User> userRepo,
            IRepository<BoardDecision> boardDecisionRepo,
            ISeriesService seriesService,
            INotificationDispatchService notificationDispatchService,
            ILogger<SeriesProposalWorkflowService> logger)
        {
            _seriesRepo = seriesRepo;
            _proposalPageRepo = proposalPageRepo;
            _userAssignmentRepo = userAssignmentRepo;
            _userRepo = userRepo;
            _boardDecisionRepo = boardDecisionRepo;
            _seriesService = seriesService;
            _notificationDispatchService = notificationDispatchService;
            _logger = logger;
        }

        public async Task<SeriesDetailResponse> SubmitForReviewAsync(Guid seriesId, Guid mangakaId)
        {
            var series = await GetSeriesAsync(seriesId);
            if (series.MangakaId != mangakaId)
                throw new UnauthorizedAccessException("Only the proposal owner can submit this proposal for review.");
            if (series.Status != SeriesStatus.Draft && series.Status != SeriesStatus.RevisionRequired)
                throw new InvalidOperationException("Only draft or revision-required proposals can be submitted for review.");

            // await EnsureMinimumProposalPagesAsync(seriesId); // Rule disabled: minimum 5 pages not required

            series.Status = SeriesStatus.UnderReview;
            series.RejectReason = null;
            series.SubmittedAt = DateTime.UtcNow;
            _seriesRepo.Update(series);
            await _seriesRepo.SaveChangeAsync();

            var tantouEditorId = await GetAssignedTantouEditorIdAsync(series.MangakaId);
            if (tantouEditorId.HasValue)
            {
                await TryNotifyUsersAsync(
                    $"Proposal '{series.Title}' was submitted for Tantou review.",
                    new[] { tantouEditorId.Value },
                    series.SeriesId);
            }
            else
            {
                _logger.LogWarning(
                    "Proposal {SeriesId} was submitted for review, but no active assigned Tantou Editor was found.",
                    series.SeriesId);
            }

            return await _seriesService.GetByIdAsync(seriesId);
        }

        public async Task<SeriesDetailResponse> RequestRevisionAsync(
            Guid seriesId,
            Guid tantouEditorId,
            RequestProposalRevisionRequest request)
        {
            var revisionReason = request.RevisionReason?.Trim();
            if (string.IsNullOrWhiteSpace(revisionReason))
                throw new ArgumentException("Revision reason is required.");

            var series = await GetSeriesAsync(seriesId);
            await EnsureAssignedTantouEditorAsync(series.MangakaId, tantouEditorId);

            if (series.Status != SeriesStatus.UnderReview)
                throw new InvalidOperationException("Only under-review proposals can be returned for revision.");

            series.Status = SeriesStatus.RevisionRequired;
            series.RejectReason = revisionReason;
            _seriesRepo.Update(series);
            await _seriesRepo.SaveChangeAsync();

            await TryNotifyUsersAsync(
                $"Proposal '{series.Title}' requires revision. Reason: {revisionReason}",
                new[] { series.MangakaId },
                series.SeriesId);

            return await _seriesService.GetByIdAsync(seriesId);
        }

        public async Task<SeriesDetailResponse> RejectAsync(Guid seriesId, Guid tantouEditorId, RejectProposalRequest request)
        {
            var rejectReason = request.RejectReason?.Trim();
            if (string.IsNullOrWhiteSpace(rejectReason))
                throw new ArgumentException("Reject reason is required.");

            var series = await GetSeriesAsync(seriesId);
            await EnsureAssignedTantouEditorAsync(series.MangakaId, tantouEditorId);

            if (series.Status != SeriesStatus.UnderReview)
                throw new InvalidOperationException("Only under-review proposals can be rejected by Tantou Editor.");

            series.Status = SeriesStatus.Rejected;// system status
            series.RejectReason = rejectReason;
            _seriesRepo.Update(series);
            await _seriesRepo.SaveChangeAsync();

            await TryNotifyUsersAsync(
                $"Proposal '{series.Title}' was rejected. Reason: {rejectReason}",
                new[] { series.MangakaId },
                series.SeriesId);

            return await _seriesService.GetByIdAsync(seriesId);
        }

        public async Task<BoardDecisionResponse> SubmitToBoardAsync(Guid seriesId, Guid tantouEditorId)
        {
            var series = await GetSeriesAsync(seriesId);
            await EnsureAssignedTantouEditorAsync(series.MangakaId, tantouEditorId);

            if (series.Status != SeriesStatus.UnderReview)
                throw new InvalidOperationException("Only under-review proposals can be submitted to the editorial board.");

            await EnsureProposalCompletenessAsync(series);

            var hasOpenDecision = await _boardDecisionRepo.GetAll()
                .AnyAsync(d => d.SeriesId == seriesId
                    && d.DecisionType == SeriesProposalDecisionType
                    && d.Status == OpenDecisionStatus
                    && d.DeletedAt == null);
            if (hasOpenDecision)
                throw new InvalidOperationException("This proposal already has an open board decision.");

            await EnsureActiveEditorialBoardRecipientAsync();

            var now = DateTime.UtcNow;
            var decision = new BoardDecision
            {
                BoardDecisionId = Guid.NewGuid(),
                SeriesId = seriesId,
                DecisionType = SeriesProposalDecisionType,
                Status = OpenDecisionStatus,
                VotingDeadline = now.AddDays(7), //Voting deadline //BR-19: Voting Window
                CreatedBy = tantouEditorId,
                CreatedAt = now
            };

            await _boardDecisionRepo.AddAsync(decision);
            series.Status = SeriesStatus.BoardVoting;
            _seriesRepo.Update(series);

            try
            {
                await _seriesRepo.SaveChangeAsync();
            }
            catch (DbUpdateException ex) when (IsOpenBoardDecisionUniqueConflict(ex))
            {
                throw new InvalidOperationException("This proposal already has an open board decision.", ex);
            }

            var dispatchResult = await _notificationDispatchService.DispatchToRoleAsync(
                new NotificationDispatchRequest
                {
                    Message = $"Proposal '{series.Title}' was submitted for editorial board voting."
                },
                UserRole.EditorialBoard.ToString());

            if (dispatchResult.Status == NotificationDispatchStatus.NoRecipients)
            {
                throw new InvalidOperationException(dispatchResult.Message);
            }

            return MapDecision(decision);
        }

        public async Task<SeriesDetailResponse> ActivateAsync(Guid seriesId, Guid tantouEditorId)
        { //BR-07 Activation Preconditions
            var series = await GetSeriesAsync(seriesId);
            await EnsureAssignedTantouEditorAsync(series.MangakaId, tantouEditorId);

            if (series.Status != SeriesStatus.Approved)
                throw new InvalidOperationException("Only proposals with Approved status can be activated.");

            // Require a finalized approved board decision: either normal majority quorum or EiC special decision.
            var hasApprovedDecision = await _boardDecisionRepo.GetAll()
                .AnyAsync(d => d.SeriesId == seriesId
                    && d.DecisionType == SeriesProposalDecisionType
                    && d.DeletedAt == null
                    && d.Result == "Approved"
                    && d.FinalizedAt != null);

            if (!hasApprovedDecision)
                throw new InvalidOperationException(
                    "Activation requires a finalized approved board decision with valid quorum or Editor-in-Chief special approval.");

            series.Status = SeriesStatus.Active;
            _seriesRepo.Update(series);
            await _seriesRepo.SaveChangeAsync();

            await TryNotifyUsersAsync(
                $"Proposal '{series.Title}' was activated as an active series.",
                new[] { series.MangakaId },
                series.SeriesId);

            return await _seriesService.GetByIdAsync(seriesId);
        }


        private async Task<MangaManagementSystem.DataAccess.Entities.Models.Series> GetSeriesAsync(Guid seriesId)
        {
            return await _seriesRepo.GetAll()
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.DeletedAt == null)
                ?? throw new KeyNotFoundException("Series not found.");
        }

        // Rule disabled: minimum 5 pages not required
        // private async Task EnsureMinimumProposalPagesAsync(Guid seriesId)
        // {
        //     var proposalPageCount = await _proposalPageRepo.GetAll()
        //         .CountAsync(p => p.SeriesId == seriesId && p.DeletedAt == null);
        //     if (proposalPageCount < MinimumProposalPageCount)
        //         throw new InvalidOperationException("At least 5 non-deleted proposal pages are required.");
        // }

        private async Task EnsureProposalCompletenessAsync(MangaManagementSystem.DataAccess.Entities.Models.Series series)
        {
            ValidateTitle(series.Title);
            ValidateSynopsis(series.Synopsis);
            ValidatePublicationType(series.PublicationType);
            await EnsureAtLeastOneGenreAsync(series.SeriesId);
            // await EnsureMinimumProposalPagesAsync(series.SeriesId); // Rule disabled: minimum 5 pages not required
        }

        private async Task EnsureAtLeastOneGenreAsync(Guid seriesId)
        {
            var hasGenre = await _seriesRepo.GetAll()
                .Where(s => s.SeriesId == seriesId && s.DeletedAt == null)
                .SelectMany(s => s.SeriesGenres)
                .AnyAsync(sg => sg.Genre.DeletedAt == null);
            if (!hasGenre)
                throw new InvalidOperationException("At least one non-deleted genre is required.");
        }

        private static void ValidateTitle(string? title)
        {
            var trimmed = title?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new InvalidOperationException("Title is required.");
            if (trimmed.Length > 100)
                throw new InvalidOperationException("Title must be 100 characters or fewer.");
        }

        private static void ValidateSynopsis(string? synopsis)
        {
            var trimmed = synopsis?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new InvalidOperationException("Synopsis is required.");
            if (trimmed.Length < 100 || trimmed.Length > 2000)
                throw new InvalidOperationException("Synopsis must be between 100 and 2000 characters.");
        }

        private static void ValidatePublicationType(string? publicationType)
        {
            var trimmed = publicationType?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new InvalidOperationException("Publication type is required.");
            if (!ValidPublicationTypes.Any(type => type.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Publication type must be Weekly, Monthly, or One-shot.");
        }

        private static bool IsOpenBoardDecisionUniqueConflict(DbUpdateException exception)
        {
            return exception.InnerException is PostgresException postgresException
                && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
                && string.Equals(
                    postgresException.ConstraintName,
                    OpenSeriesProposalDecisionIndexName,
                    StringComparison.Ordinal);
        }

        private async Task EnsureAssignedTantouEditorAsync(Guid mangakaId, Guid tantouEditorId)
        {
            var isAssigned = await _userAssignmentRepo.GetAll()
                .Include(a => a.FromUser).ThenInclude(u => u.Role)
                .AnyAsync(a => a.FromUserId == tantouEditorId
                    && a.ToUserId == mangakaId
                    && a.FromUser.Role.RoleName == UserRole.TantouEditor.ToString()
                    && a.UnassignedAt == null
                    && a.DeletedAt == null);
            if (!isAssigned)
                throw new ForbiddenAccessException("Only the assigned Tantou Editor can review this proposal.");
        }

        private async Task EnsureActiveEditorialBoardRecipientAsync()
        {
            var hasActiveEditorialBoardRecipient = await _userRepo.GetAll()
                .Include(u => u.Role)
                .AnyAsync(u => u.DeletedAt == null
                    && u.Role.DeletedAt == null
                    && u.Role.RoleName == UserRole.EditorialBoard.ToString());

            if (!hasActiveEditorialBoardRecipient)
                throw new InvalidOperationException(
                    $"No active users were found for role '{UserRole.EditorialBoard}'.");
        }

        private async Task<Guid?> GetAssignedTantouEditorIdAsync(Guid mangakaId)
        {
            return await _userAssignmentRepo.GetAll()
                .Include(a => a.FromUser)
                    .ThenInclude(u => u.Role)
                .Where(a => a.ToUserId == mangakaId
                    && a.UnassignedAt == null
                    && a.DeletedAt == null
                    && a.FromUser.DeletedAt == null
                    && a.FromUser.Role.DeletedAt == null
                    && a.FromUser.Role.RoleName == UserRole.TantouEditor.ToString())
                .Select(a => (Guid?)a.FromUserId)
                .FirstOrDefaultAsync();
        }

        private async Task TryNotifyUsersAsync(string message, IEnumerable<Guid> userIds, Guid seriesId)
        {
            try
            {
                var result = await _notificationDispatchService.DispatchToUsersAsync(
                    new NotificationDispatchRequest { Message = TruncateNotificationMessage(message) },
                    userIds);

                if (result.Status == NotificationDispatchStatus.NoRecipients)
                {
                    _logger.LogWarning(
                        "Proposal {SeriesId} notification had no recipients: {Message}",
                        seriesId,
                        result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proposal {SeriesId} notification dispatch failed.", seriesId);
            }
        }

        private static string TruncateNotificationMessage(string message)
        {
            return message.Length <= 1000 ? message : message[..1000];
        }

        private static BoardDecisionResponse MapDecision(BoardDecision decision) => new()
        {
            BoardDecisionId = decision.BoardDecisionId,
            SeriesId = decision.SeriesId,
            DecisionType = decision.DecisionType,
            Status = decision.Status,
            Result = decision.Result,
            VotingDeadline = decision.VotingDeadline,
            FinalizedAt = decision.FinalizedAt,
            CreatedAt = decision.CreatedAt,
            CreatedBy = decision.CreatedBy,
            ExtensionCount = decision.ExtensionCount,
            ExtendedBy = decision.ExtendedBy,
            ExtendedAt = decision.ExtendedAt,
            ExtensionReason = decision.ExtensionReason,
            SpecialDecisionBy = decision.SpecialDecisionBy,
            SpecialDecisionAt = decision.SpecialDecisionAt,
            SpecialDecisionReason = decision.SpecialDecisionReason,
            VoteCount = decision.BoardVotes?.Count(v => v.DeletedAt == null) ?? 0
        };
    }
}
