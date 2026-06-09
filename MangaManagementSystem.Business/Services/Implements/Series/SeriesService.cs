using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class SeriesService : ISeriesService
    {
        private readonly IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> _seriesRepo;
        private readonly IRepository<SeriesGenre> _seriesGenreRepo;

        public SeriesService(IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> seriesRepo, IRepository<SeriesGenre> seriesGenreRepo)
        {
            _seriesRepo = seriesRepo;
            _seriesGenreRepo = seriesGenreRepo;
        }

        public async Task<IEnumerable<SeriesResponse>> GetAllAsync(string? status = null)
        {
            var query = _seriesRepo.GetAll()
                .Include(s => s.Mangaka)
                .Include(s => s.SeriesGenres).ThenInclude(sg => sg.Genre)
                .Where(s => s.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("Proposed", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(s =>
                        s.Status == SeriesStatus.Draft
                        || s.Status == SeriesStatus.UnderReview
                        || s.Status == SeriesStatus.BoardVoting);
                }
                else if (Enum.TryParse<SeriesStatus>(status, ignoreCase: true, out var parsedStatus))
                {
                    query = query.Where(s => s.Status == parsedStatus);
                }
                else
                {
                    query = query.Where(s => s.Status.ToString() == status);
                }
            }

            return await query.Select(s => MapToResponse(s)).ToListAsync();
        }

        public async Task<SeriesDetailResponse> GetByIdAsync(Guid id)
        {
            var s = await _seriesRepo.GetAll()
                .Include(s => s.Mangaka)
                .Include(s => s.SeriesGenres).ThenInclude(sg => sg.Genre)
                .Include(s => s.ProposalPages)
                .FirstOrDefaultAsync(s => s.SeriesId == id && s.DeletedAt == null)
                ?? throw new KeyNotFoundException("Series not found.");

            var detail = new SeriesDetailResponse
            {
                SeriesId = s.SeriesId, MangakaId = s.MangakaId, MangakaName = s.Mangaka.DisplayName,
                Title = s.Title, Synopsis = s.Synopsis, PublicationType = s.PublicationType,
                Status = s.Status.ToString(),
                RankingScore = s.RankingScore, CreatedAt = s.CreatedAt,
                SubmittedAt = s.SubmittedAt, RejectReason = s.RejectReason,
                Genres = s.SeriesGenres.Where(sg => sg.Genre.DeletedAt == null).Select(sg => sg.Genre.Title).ToList(),
                ProposalPages = s.ProposalPages.Where(p => p.DeletedAt == null)
                    .Select(p => new ProposalPageResponse { ProposalPageId = p.ProposalPageId, SeriesId = p.SeriesId, PageNo = p.PageNo, PreviewFileAssetId = p.PreviewFileAssetId, CreatedAt = p.CreatedAt }).ToList()
            };
            return detail;
        }

        public async Task<IEnumerable<SeriesResponse>> GetByMangakaAsync(Guid mangakaId)
        {
            return await _seriesRepo.GetAll()
                .Include(s => s.Mangaka)
                .Include(s => s.SeriesGenres).ThenInclude(sg => sg.Genre)
                .Where(s => s.MangakaId == mangakaId && s.DeletedAt == null)
                .Select(s => MapToResponse(s))
                .ToListAsync();
        }

        public async Task<SeriesResponse> CreateAsync(Guid mangakaId, CreateSeriesRequest request)
        {
            // BR-19: no active pending proposal for this Mangaka
            var hasPending = await _seriesRepo.GetAll()
                .AnyAsync(s => s.MangakaId == mangakaId
                    && (s.Status == SeriesStatus.Draft
                        || s.Status == SeriesStatus.UnderReview
                        || s.Status == SeriesStatus.BoardVoting)
                    && s.DeletedAt == null);
            if (hasPending)
                throw new InvalidOperationException("You already have a pending proposal.");

            var series = new MangaManagementSystem.DataAccess.Entities.Models.Series
            {
                MangakaId = mangakaId,
                Title = request.Title,
                Synopsis = request.Synopsis,
                PublicationType = request.PublicationType,
                Status = SeriesStatus.Draft,
                SubmittedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                SourceZipFileAssetId = request.SourceZipFileAssetId
            };
            await _seriesRepo.AddAsync(series);

            foreach (var genreId in request.GenreIds)
                await _seriesGenreRepo.AddAsync(new SeriesGenre { SeriesId = series.SeriesId, GenreId = genreId });

            await _seriesRepo.SaveChangeAsync();

            return await GetByIdAsync(series.SeriesId) as SeriesResponse
                   ?? throw new Exception("Failed to retrieve created series.");
        }

        public async Task<SeriesResponse> UpdateAsync(Guid id, UpdateSeriesRequest request)
        {
            var series = await _seriesRepo.GetAll()
                .FirstOrDefaultAsync(s => s.SeriesId == id && s.DeletedAt == null)
                ?? throw new KeyNotFoundException("Series not found.");

            if (request.Title != null) series.Title = request.Title;
            if (request.Synopsis != null) series.Synopsis = request.Synopsis;
            if (request.Status != null)
            {
                if (!Enum.TryParse<SeriesStatus>(request.Status, ignoreCase: true, out var parsedStatus))
                    throw new ArgumentException("Invalid series status.");
                series.Status = parsedStatus;
            }
            if (request.RejectReason != null) series.RejectReason = request.RejectReason;
            if (request.PublicationType != null) series.PublicationType = request.PublicationType;

            _seriesRepo.Update(series);
            await _seriesRepo.SaveChangeAsync();
            return await GetByIdAsync(id) as SeriesResponse ?? throw new Exception("Update failed.");
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var series = await _seriesRepo.GetAll()
                .Include(s => s.SeriesGenres)
                .Include(s => s.ProposalPages)
                .Include(s => s.Chapters)
                .Include(s => s.BoardDecisions)
                .Include(s => s.VoteRecords)
                .Include(s => s.RankingSnapshots)
                .Include(s => s.Escalations)
                .FirstOrDefaultAsync(s => s.SeriesId == id && s.DeletedAt == null)
                ?? throw new KeyNotFoundException("Series not found.");

            var now = DateTime.UtcNow;
            series.DeletedAt = now;
            foreach (var sg in series.SeriesGenres.Where(x => x.Genre?.DeletedAt == null)) { /* no DeletedAt on SeriesGenre — use hard delete or skip */ }
            foreach (var p in series.ProposalPages.Where(x => x.DeletedAt == null)) p.DeletedAt = now;
            foreach (var c in series.Chapters.Where(x => x.DeletedAt == null)) c.DeletedAt = now;
            foreach (var bd in series.BoardDecisions.Where(x => x.DeletedAt == null)) bd.DeletedAt = now;
            foreach (var vr in series.VoteRecords.Where(x => x.DeletedAt == null)) vr.DeletedAt = now;
            foreach (var rs in series.RankingSnapshots.Where(x => x.DeletedAt == null)) rs.DeletedAt = now;
            foreach (var e in series.Escalations.Where(x => x.DeletedAt == null)) e.DeletedAt = now;

            _seriesRepo.Update(series);
            await _seriesRepo.SaveChangeAsync();
        }

        private static SeriesResponse MapToResponse(MangaManagementSystem.DataAccess.Entities.Models.Series s) => new()
        {
            SeriesId = s.SeriesId, MangakaId = s.MangakaId, MangakaName = s.Mangaka.DisplayName,
            Title = s.Title, Synopsis = s.Synopsis, PublicationType = s.PublicationType,
            Status = s.Status.ToString(),
            RankingScore = s.RankingScore, CreatedAt = s.CreatedAt,
            SubmittedAt = s.SubmittedAt, RejectReason = s.RejectReason,
            Genres = s.SeriesGenres.Where(sg => sg.Genre.DeletedAt == null).Select(sg => sg.Genre.Title).ToList()
        };
    }
}
