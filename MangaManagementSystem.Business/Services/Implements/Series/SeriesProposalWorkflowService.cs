using MangaManagement.DataAccess.DbContexts;
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
using Npgsql;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class SeriesProposalWorkflowService : ISeriesProposalWorkflowService
    {
        private const int MinimumProposalPageCount = 5;

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
        private readonly IRepository<BoardDecision> _boardDecisionRepo;
        private readonly ISeriesService _seriesService;
        private readonly INotificationDispatchService _notificationDispatchService;
        private readonly MangaDbContext _dbContext;

        public SeriesProposalWorkflowService(
            IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> seriesRepo,
            IRepository<ProposalPage> proposalPageRepo,
            IRepository<UserAssignment> userAssignmentRepo,
            IRepository<BoardDecision> boardDecisionRepo,
            ISeriesService seriesService,
            INotificationDispatchService notificationDispatchService,
            MangaDbContext dbContext)
        {
            _seriesRepo = seriesRepo;
            _proposalPageRepo = proposalPageRepo;
            _userAssignmentRepo = userAssignmentRepo;
            _boardDecisionRepo = boardDecisionRepo;
            _seriesService = seriesService;
            _notificationDispatchService = notificationDispatchService;
            _dbContext = dbContext;
        }

        public async Task<SeriesDetailResponse> SubmitForReviewAsync(Guid seriesId, Guid mangakaId)
        {
            var series = await GetSeriesAsync(seriesId);
            if (series.MangakaId != mangakaId)
                throw new UnauthorizedAccessException("Only the proposal owner can submit this proposal for review.");
            if (series.Status != SeriesStatus.Draft)
                throw new InvalidOperationException("Only draft proposals can be submitted for review.");

            await EnsureMinimumProposalPagesAsync(seriesId);

            series.Status = SeriesStatus.UnderReview;
            series.SubmittedAt = DateTime.UtcNow;
            _seriesRepo.Update(series);
            await _seriesRepo.SaveChangeAsync();

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

            series.Status = SeriesStatus.Rejected;
            series.RejectReason = rejectReason;
            _seriesRepo.Update(series);
            await _seriesRepo.SaveChangeAsync();

            return await _seriesService.GetByIdAsync(seriesId);
        }

        public async Task<BoardDecisionResponse> SubmitToBoardAsync(Guid seriesId, Guid tantouEditorId)
        {
            var series = await GetSeriesAsync(seriesId);
            await EnsureAssignedTantouEditorAsync(series.MangakaId, tantouEditorId);

            if (series.Status != SeriesStatus.UnderReview)
                throw new InvalidOperationException("Only under-review proposals can be submitted to the editorial board.");

            await EnsureProposalCompletenessAsync(series);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var hasOpenDecision = await _boardDecisionRepo.GetAll()
                .AnyAsync(d => d.SeriesId == seriesId
                    && d.DecisionType == SeriesProposalDecisionType
                    && d.Status == OpenDecisionStatus
                    && d.DeletedAt == null);
            if (hasOpenDecision)
                throw new InvalidOperationException("This proposal already has an open board decision.");

            var now = DateTime.UtcNow;
            var decision = new BoardDecision
            {
                BoardDecisionId = Guid.NewGuid(),
                SeriesId = seriesId,
                DecisionType = SeriesProposalDecisionType,
                Status = OpenDecisionStatus,
                VotingDeadline = now.AddDays(7),
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
                    Title = "Series proposal ready for board voting",
                    Message = $"Series proposal '{series.Title}' has been submitted for editorial board voting.",
                    Type = SeriesProposalDecisionType,
                    Link = $"/api/board-decisions/{decision.BoardDecisionId}",
                    Priority = "Normal"
                },
                UserRole.EditorialBoard.ToString());

            if (dispatchResult.Status == NotificationDispatchStatus.NoRecipients)
            {
                throw new InvalidOperationException(dispatchResult.Message);
            }

            await transaction.CommitAsync();

            return MapDecision(decision);
        }

        public async Task<SeriesDetailResponse> ActivateAsync(Guid seriesId, Guid tantouEditorId)
        {
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

            return await _seriesService.GetByIdAsync(seriesId);
        }


        private async Task<MangaManagementSystem.DataAccess.Entities.Models.Series> GetSeriesAsync(Guid seriesId)
        {
            return await _seriesRepo.GetAll()
                .FirstOrDefaultAsync(s => s.SeriesId == seriesId && s.DeletedAt == null)
                ?? throw new KeyNotFoundException("Series not found.");
        }

        private async Task EnsureMinimumProposalPagesAsync(Guid seriesId)
        {
            var proposalPageCount = await _proposalPageRepo.GetAll()
                .CountAsync(p => p.SeriesId == seriesId && p.DeletedAt == null);
            if (proposalPageCount < MinimumProposalPageCount)
                throw new InvalidOperationException("At least 5 non-deleted proposal pages are required.");
        }

        private async Task EnsureProposalCompletenessAsync(MangaManagementSystem.DataAccess.Entities.Models.Series series)
        {
            ValidateTitle(series.Title);
            ValidateSynopsis(series.Synopsis);
            ValidatePublicationType(series.PublicationType);
            await EnsureAtLeastOneGenreAsync(series.SeriesId);
            await EnsureMinimumProposalPagesAsync(series.SeriesId);
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
