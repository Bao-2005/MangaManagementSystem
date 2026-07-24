using MangaManagementSystem.Business.DTOs.Requests.Ranking;
using MangaManagementSystem.Business.DTOs.Responses.Ranking;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Services.Interfaces.Ranking;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using SeriesEntity = MangaManagementSystem.DataAccess.Entities.Models.Series;

namespace MangaManagementSystem.Business.Services.Implements.Ranking
{
    public class RankingWorkflowService : IRankingWorkflowService
    {
        private const string RankingEliminationDecisionType = "RankingElimination";
        private const string OpenDecisionStatus = "Open";

        private readonly IRepository<VoteRecord> _voteRecordRepository;
        private readonly IRepository<RankingSnapshot> _rankingSnapshotRepository;
        private readonly IRepository<BoardDecision> _boardDecisionRepository;
        private readonly IRepository<SeriesEntity> _seriesRepository;
        private readonly IRepository<User> _userRepository;

        public RankingWorkflowService(
            IRepository<VoteRecord> voteRecordRepository,
            IRepository<RankingSnapshot> rankingSnapshotRepository,
            IRepository<BoardDecision> boardDecisionRepository,
            IRepository<SeriesEntity> seriesRepository,
            IRepository<User> userRepository)
        {
            _voteRecordRepository = voteRecordRepository;
            _rankingSnapshotRepository = rankingSnapshotRepository;
            _boardDecisionRepository = boardDecisionRepository;
            _seriesRepository = seriesRepository;
            _userRepository = userRepository;
        }

        public async Task<RankingVoteRecordResponse> CreateVoteRecordAsync(
            Guid actorId,
            CreateRankingVoteRecordRequest request)
        {
            await EnsureActiveEditorialBoardAsync(actorId);

            var period = RankingPeriodHelper.NormalizeMonthlyPeriod(request.Period);
            ValidateCounts(request.ReaderCount, request.VoteCount);

            var series = await _seriesRepository.GetAll()
                .FirstOrDefaultAsync(x => x.SeriesId == request.SeriesId && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Series not found.");

            if (series.Status != SeriesStatus.Active)
                throw new InvalidOperationException("Only active series can be ranked.");

            var duplicateExists = await _voteRecordRepository.GetAll()
                .AnyAsync(x => x.SeriesId == request.SeriesId
                    && x.Period == period
                    && x.DeletedAt == null);
            if (duplicateExists)
                throw new InvalidOperationException("Vote record already exists for this series and period.");

            var voteRecord = new VoteRecord
            {
                SeriesId = request.SeriesId,
                Period = period,
                ReaderCount = request.ReaderCount,
                VoteCount = request.VoteCount,
                Status = RankingConstants.VoteRecordStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _voteRecordRepository.AddAsync(voteRecord);
            await _voteRecordRepository.SaveChangeAsync();

            voteRecord.Series = series;
            return MapVoteRecord(voteRecord);
        }

        public async Task<RankingRecalculationResponse> ConfirmVoteRecordAsync(Guid actorId, Guid voteRecordId)
        {
            var actor = await EnsureActiveEditorialBoardAsync(actorId);

            var voteRecord = await _voteRecordRepository.GetAll()
                .Include(x => x.Series)
                .Include(x => x.Confirmer)
                .FirstOrDefaultAsync(x => x.VoteRecordId == voteRecordId && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Vote record not found.");

            if (voteRecord.Status != RankingConstants.VoteRecordStatus.Pending)
                throw new InvalidOperationException("Only pending vote records can be confirmed.");

            if (voteRecord.Series.DeletedAt != null || voteRecord.Series.Status != SeriesStatus.Active)
                throw new InvalidOperationException("Only active series can be ranked.");

            voteRecord.Status = RankingConstants.VoteRecordStatus.Confirmed;
            voteRecord.ConfirmedBy = actorId;
            voteRecord.ConfirmedAt = DateTime.UtcNow;
            voteRecord.Confirmer = actor;

            var totalRanked = await RecalculatePeriodAsync(voteRecord.Period);
            await _voteRecordRepository.SaveChangeAsync();

            var snapshots = await GetRankingsAsync(voteRecord.Period);
            return new RankingRecalculationResponse
            {
                Period = voteRecord.Period,
                TotalRanked = totalRanked,
                Snapshots = snapshots
            };
        }

        public async Task<BoardDecisionResponse> CreateEliminationDecisionAsync(
            Guid actorId,
            Guid rankingSnapshotId,
            CreateRankingEliminationDecisionRequest request)
        {
            await EnsureActiveEditorialBoardAsync(actorId);

            var snapshot = await _rankingSnapshotRepository.GetAll()
                .Include(x => x.Series)
                .FirstOrDefaultAsync(x => x.RankingSnapshotId == rankingSnapshotId && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Ranking snapshot not found.");

            if (!snapshot.IsBottom20Percent)
                throw new InvalidOperationException("Only bottom 20 percent ranking snapshots can have elimination decisions.");

            if (snapshot.Series.DeletedAt != null || snapshot.Series.Status != SeriesStatus.Active)
                throw new InvalidOperationException("Only active series can have elimination decisions.");

            if (request.VotingDeadline <= DateTime.UtcNow)
                throw new ArgumentException("Voting deadline must be in the future.");

            var duplicateExists = await _boardDecisionRepository.GetAll()
                .AnyAsync(x => x.SeriesId == snapshot.SeriesId
                    && x.DecisionType == RankingEliminationDecisionType
                    && x.Status == OpenDecisionStatus
                    && x.DeletedAt == null);
            if (duplicateExists)
                throw new InvalidOperationException("An open ranking elimination decision already exists for this series.");

            var now = DateTime.UtcNow;
            var decision = new BoardDecision
            {
                SeriesId = snapshot.SeriesId,
                DecisionType = RankingEliminationDecisionType,
                Status = OpenDecisionStatus,
                VotingDeadline = request.VotingDeadline,
                CreatedBy = actorId,
                CreatedAt = now
            };

            await _boardDecisionRepository.AddAsync(decision);
            await _boardDecisionRepository.SaveChangeAsync();

            return MapDecision(decision);
        }

        public async Task<IReadOnlyList<RankingSnapshotDetailResponse>> GetRankingsAsync(string period)
        {
            var normalizedPeriod = RankingPeriodHelper.NormalizeMonthlyPeriod(period);

            var snapshots = await _rankingSnapshotRepository.GetAll()
                .Include(x => x.Series)
                .Where(x => x.Period == normalizedPeriod && x.DeletedAt == null)
                .OrderBy(x => x.RankNo)
                .ToListAsync();

            return snapshots.Select(MapSnapshot).ToList();
        }

        public async Task<IReadOnlyList<RankingSnapshotDetailResponse>> GetSeriesRankingHistoryAsync(Guid seriesId)
        {
            var exists = await _seriesRepository.GetAll()
                .AnyAsync(x => x.SeriesId == seriesId && x.DeletedAt == null);
            if (!exists)
                throw new KeyNotFoundException("Series not found.");

            var snapshots = await _rankingSnapshotRepository.GetAll()
                .Include(x => x.Series)
                .Where(x => x.SeriesId == seriesId && x.DeletedAt == null)
                .ToListAsync();

            return snapshots
                .OrderByDescending(x => ParsePeriod(x.Period))
                .ThenBy(x => x.RankNo)
                .Select(MapSnapshot)
                .ToList();
        }

        private async Task<int> RecalculatePeriodAsync(string period)
        {
            var confirmedRecords = await _voteRecordRepository.GetAll()
                .Include(x => x.Series)
                .Where(x => x.Period == period
                    && x.Status == RankingConstants.VoteRecordStatus.Confirmed
                    && x.DeletedAt == null
                    && x.Series.DeletedAt == null
                    && x.Series.Status == SeriesStatus.Active)
                .ToListAsync();

            var rankedRecords = confirmedRecords
                .Select(x => new RankingCandidate(x, CalculateScore(x.ReaderCount, x.VoteCount)))
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.VoteRecord.VoteCount)
                .ThenBy(x => x.VoteRecord.Series.Title)
                .ThenBy(x => x.VoteRecord.SeriesId)
                .ToList();

            var existingSnapshots = await _rankingSnapshotRepository.GetAll()
                .Include(x => x.Series)
                .Where(x => x.Period == period && x.DeletedAt == null)
                .ToListAsync();

            var snapshotsBySeries = existingSnapshots.ToDictionary(x => x.SeriesId);
            var activeSeriesIds = rankedRecords.Select(x => x.VoteRecord.SeriesId).ToHashSet();
            var now = DateTime.UtcNow;
            var bottomCount = rankedRecords.Count >= 5
                ? (int)Math.Ceiling(rankedRecords.Count * 0.2m)
                : 0;

            for (var index = 0; index < rankedRecords.Count; index++)
            {
                var candidate = rankedRecords[index];
                var rankNo = index + 1;
                var isBottom = bottomCount > 0 && rankNo > rankedRecords.Count - bottomCount;

                if (!snapshotsBySeries.TryGetValue(candidate.VoteRecord.SeriesId, out var snapshot))
                {
                    snapshot = new RankingSnapshot
                    {
                        SeriesId = candidate.VoteRecord.SeriesId,
                        Period = period,
                        CreatedAt = now,
                        Series = candidate.VoteRecord.Series
                    };
                    await _rankingSnapshotRepository.AddAsync(snapshot);
                    snapshotsBySeries[snapshot.SeriesId] = snapshot;
                }

                snapshot.VoteRecordId = candidate.VoteRecord.VoteRecordId;
                snapshot.Score = candidate.Score;
                snapshot.ReaderCount = candidate.VoteRecord.ReaderCount;
                snapshot.VoteCount = candidate.VoteRecord.VoteCount;
                snapshot.RankNo = rankNo;
                snapshot.IsBottom20Percent = isBottom;
                snapshot.DeletedAt = null;
            }

            foreach (var staleSnapshot in existingSnapshots.Where(x => !activeSeriesIds.Contains(x.SeriesId)))
            {
                staleSnapshot.DeletedAt = now;
            }

            return rankedRecords.Count;
        }

        private async Task<User> EnsureActiveEditorialBoardAsync(Guid actorId)
        {
            var actor = await _userRepository.GetAll()
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == actorId && x.DeletedAt == null)
                ?? throw new UnauthorizedAccessException("User not found.");

            if (actor.Role.DeletedAt != null || actor.Role.RoleName != UserRole.EditorialBoard.ToString())
                throw new UnauthorizedAccessException("Only active Editorial Board members can manage ranking vote records.");

            return actor;
        }

        private static void ValidateCounts(int readerCount, int voteCount)
        {
            if (readerCount < 0)
                throw new ArgumentException("ReaderCount cannot be negative.");

            if (voteCount < 0)
                throw new ArgumentException("VoteCount cannot be negative.");

            if (voteCount > readerCount)
                throw new ArgumentException("VoteCount cannot exceed ReaderCount.");
        }

        private static RankingVoteRecordResponse MapVoteRecord(VoteRecord voteRecord)
        {
            return new RankingVoteRecordResponse
            {
                VoteRecordId = voteRecord.VoteRecordId,
                SeriesId = voteRecord.SeriesId,
                SeriesTitle = voteRecord.Series?.Title ?? string.Empty,
                Period = voteRecord.Period,
                ReaderCount = voteRecord.ReaderCount,
                VoteCount = voteRecord.VoteCount,
                Score = CalculateScore(voteRecord.ReaderCount, voteRecord.VoteCount),
                Status = voteRecord.Status,
                ConfirmedBy = voteRecord.ConfirmedBy,
                ConfirmerName = voteRecord.Confirmer?.DisplayName,
                ConfirmedAt = voteRecord.ConfirmedAt,
                CreatedAt = voteRecord.CreatedAt
            };
        }

        private static RankingSnapshotDetailResponse MapSnapshot(RankingSnapshot snapshot)
        {
            return new RankingSnapshotDetailResponse
            {
                RankingSnapshotId = snapshot.RankingSnapshotId,
                SeriesId = snapshot.SeriesId,
                SeriesTitle = snapshot.Series?.Title ?? string.Empty,
                VoteRecordId = snapshot.VoteRecordId,
                Period = snapshot.Period,
                RankNo = snapshot.RankNo,
                Score = snapshot.Score,
                ReaderCount = snapshot.ReaderCount,
                VoteCount = snapshot.VoteCount,
                IsBottom20Percent = snapshot.IsBottom20Percent,
                CreatedAt = snapshot.CreatedAt
            };
        }

        private static BoardDecisionResponse MapDecision(BoardDecision decision)
        {
            return new BoardDecisionResponse
            {
                BoardDecisionId = decision.BoardDecisionId,
                SeriesId = decision.SeriesId,
                DecisionType = decision.DecisionType,
                Status = decision.Status,
                Result = decision.Result,
                VotingDeadline = decision.VotingDeadline,
                FinalizedAt = decision.FinalizedAt,
                CreatedBy = decision.CreatedBy,
                ExtensionCount = decision.ExtensionCount,
                ExtendedBy = decision.ExtendedBy,
                ExtendedAt = decision.ExtendedAt,
                ExtensionReason = decision.ExtensionReason,
                SpecialDecisionBy = decision.SpecialDecisionBy,
                SpecialDecisionAt = decision.SpecialDecisionAt,
                SpecialDecisionReason = decision.SpecialDecisionReason,
                CreatedAt = decision.CreatedAt,
                VoteCount = decision.BoardVotes.Count(v => v.DeletedAt == null)
            };
        }

        private static decimal CalculateScore(int readerCount, int voteCount)
        {
            return readerCount == 0
                ? 0
                : Math.Round((decimal)voteCount / readerCount * 100, 2);
        }

        private static DateOnly ParsePeriod(string period)
        {
            return DateOnly.ParseExact(
                period,
                RankingConstants.PeriodFormat,
                CultureInfo.InvariantCulture);
        }

        private sealed record RankingCandidate(VoteRecord VoteRecord, decimal Score);
    }
}
