using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class SeriesService : ISeriesService
    {
        //private const int MinimumSamplePageCount = 5;

        private static readonly string[] ValidPublicationTypes =
        {
            "Weekly",
            "Monthly",
            "One-shot"
        };

        private static readonly HashSet<string> ValidSamplePageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp"
        };

        private static readonly HashSet<string> ValidSamplePageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        private static readonly HashSet<string> ValidSourceFileExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".zip",
            ".rar",
            ".psd",
            ".clip",
            ".ai"
        };

        private static readonly HashSet<string> ValidSourceFileMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/zip",
            "application/x-zip-compressed",
            "application/vnd.rar",
            "application/x-rar-compressed",
            "image/vnd.adobe.photoshop",
            "image/x-photoshop",
            "application/photoshop",
            "application/postscript",
            "application/illustrator",
            "application/pdf",
            "application/octet-stream"
        };

        private readonly IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> _seriesRepo;
        private readonly IRepository<SeriesGenre> _seriesGenreRepo;
        private readonly IRepository<Genre> _genreRepo;
        private readonly IRepository<FileAsset> _fileAssetRepo;
        private readonly IRepository<ProposalPage> _proposalPageRepo;
        private readonly string _supabaseUrl;

        public SeriesService(
            IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> seriesRepo,
            IRepository<SeriesGenre> seriesGenreRepo,
            IRepository<Genre> genreRepo,
            IRepository<FileAsset> fileAssetRepo,
            IRepository<ProposalPage> proposalPageRepo,
            IConfiguration configuration)
        {
            _seriesRepo = seriesRepo;
            _seriesGenreRepo = seriesGenreRepo;
            _genreRepo = genreRepo;
            _fileAssetRepo = fileAssetRepo;
            _proposalPageRepo = proposalPageRepo;
            _supabaseUrl = (configuration["Supabase:Url"] ?? string.Empty).TrimEnd('/');
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
                        || s.Status == SeriesStatus.RevisionRequired
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
                .Include(s => s.SourceZipFileAsset)
                .Include(s => s.CoverImageFileAsset)
                .FirstOrDefaultAsync(s => s.SeriesId == id && s.DeletedAt == null)
                ?? throw new KeyNotFoundException("Series not found.");

            var detail = new SeriesDetailResponse
            {
                SeriesId = s.SeriesId,
                MangakaId = s.MangakaId,
                MangakaName = s.Mangaka.DisplayName,
                Title = s.Title,
                Synopsis = s.Synopsis,
                PublicationType = s.PublicationType,
                Status = s.Status.ToString(),
                RankingScore = s.RankingScore,
                CreatedAt = s.CreatedAt,
                SubmittedAt = s.SubmittedAt,
                RejectReason = s.RejectReason,
                Genres = s.SeriesGenres.Where(sg => sg.Genre.DeletedAt == null).Select(sg => sg.Genre.Title).ToList(),
                ProposalPages = s.ProposalPages.Where(p => p.DeletedAt == null)
                    .Select(p => new ProposalPageResponse { ProposalPageId = p.ProposalPageId, SeriesId = p.SeriesId, PageNo = p.PageNo, PreviewFileAssetId = p.PreviewFileAssetId, CreatedAt = p.CreatedAt }).ToList(),
                SourceZipFileAssetId = s.SourceZipFileAssetId,
                CoverImageFileAssetId = s.CoverImageFileAssetId,
                SourceZipPublicUrl = (s.SourceZipFileAsset == null || string.IsNullOrEmpty(_supabaseUrl)) ? null : (_supabaseUrl + "/storage/v1/object/public/" + s.SourceZipFileAsset.BucketName + "/" + s.SourceZipFileAsset.ObjectPath)
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
            var title = ValidateTitle(request.Title);// BR-06: Proposal Validation Requirements
            var synopsis = ValidateSynopsis(request.Synopsis);
            var publicationType = ValidatePublicationType(request.PublicationType);
            var genreIds = await ValidateGenresAsync(request.GenreIds);
            var samplePageFileAssetIds = await ValidateSamplePagesAsync(request.SamplePageFileAssetIds);
            await ValidateSourceZipAsync(request.SourceZipFileAssetId);

            // BR-19: no active pending proposal for this Mangaka -> old
            //BR-11: Single Active Proposal Limit
            var hasPending = await _seriesRepo.GetAll()
                .AnyAsync(s => s.MangakaId == mangakaId
                    && (//s.Status == SeriesStatus.Draft ||
                        s.Status == SeriesStatus.UnderReview
                        || s.Status == SeriesStatus.RevisionRequired
                        || s.Status == SeriesStatus.BoardVoting)
                    && s.DeletedAt == null);
            if (hasPending)
                throw new InvalidOperationException("You already have a pending proposal.");

            // BR-17: proposal title cannot match an active series title. -> old
            // BR-09: Unique Series Title
            await EnsureTitleDoesNotMatchActiveSeriesAsync(title);

            var now = DateTime.UtcNow;
            var series = new MangaManagementSystem.DataAccess.Entities.Models.Series
            {
                SeriesId = Guid.NewGuid(),
                MangakaId = mangakaId,
                Title = title,
                Synopsis = synopsis,
                PublicationType = publicationType,
                Status = SeriesStatus.Draft,
                SubmittedAt = now,
                CreatedAt = now,
                SourceZipFileAssetId = request.SourceZipFileAssetId,
                CoverImageFileAssetId = request.CoverImageFileAssetId
            };
            await _seriesRepo.AddAsync(series);

            foreach (var genreId in genreIds)
                await _seriesGenreRepo.AddAsync(new SeriesGenre { SeriesId = series.SeriesId, GenreId = genreId });

            var proposalPages = samplePageFileAssetIds.Select((fileAssetId, index) => new ProposalPage
            {
                SeriesId = series.SeriesId,
                PageNo = index + 1,
                PreviewFileAssetId = fileAssetId,
                CreatedAt = now
            });
            await _proposalPageRepo.AddRangeAsync(proposalPages);

            await _seriesRepo.SaveChangeAsync();

            return await GetByIdAsync(series.SeriesId) as SeriesResponse
                   ?? throw new Exception("Failed to retrieve created series.");
        }

        public async Task<SeriesResponse> UpdateAsync(Guid id, Guid mangakaId, UpdateSeriesRequest request)
        {
            var series = await _seriesRepo.GetAll()
                .FirstOrDefaultAsync(s => s.SeriesId == id && s.DeletedAt == null)
                ?? throw new KeyNotFoundException("Series not found.");

            if (series.MangakaId != mangakaId)
                throw new UnauthorizedAccessException("Only the proposal owner can update this series proposal.");
            if (series.Status != SeriesStatus.Draft && series.Status != SeriesStatus.RevisionRequired)
                throw new InvalidOperationException("Only draft or revision-required proposals can be updated through this endpoint.");

            if (request.Title != null)
            {
                var title = ValidateTitle(request.Title);
                await EnsureTitleDoesNotMatchActiveSeriesAsync(title, series.SeriesId);
                series.Title = title;
            }

            if (request.Synopsis != null) series.Synopsis = ValidateSynopsis(request.Synopsis);
            if (request.PublicationType != null) series.PublicationType = ValidatePublicationType(request.PublicationType);
            if (request.GenreIds != null)
            {
                var genresId = await ValidateGenresAsync(request.GenreIds);
                var oldSeriesGenreIds = await _seriesGenreRepo.GetAll().Where(x => x.SeriesId == series.SeriesId).ToListAsync();
                var genresToDelete = oldSeriesGenreIds.Where(x => !genresId.Contains(x.GenreId));
                if(genresToDelete.Count() > 0) _seriesGenreRepo.DeleteRange(genresToDelete);
               
                var newSeriesGenres = genresId.Where(genreId => !oldSeriesGenreIds.Select(x => x.GenreId).Contains(genreId)).Select(x => new SeriesGenre
                {
                    SeriesId = series.SeriesId,
                    GenreId = x
                }).ToList();
                if(newSeriesGenres.Count() > 0) await _seriesGenreRepo.AddRangeAsync(newSeriesGenres);
            }

            if (request.CoverImageFileAssetId != null)
            {

                series.CoverImageFileAssetId = await ValidateCoverImageAsync(request.CoverImageFileAssetId, id);
            }
            if (request.SourceZipFileAssetId != null)
            {
                await ValidateSourceZipAsync(request.SourceZipFileAssetId, id);
                series.SourceZipFileAssetId = request.SourceZipFileAssetId;
            }
            if(request.SamplePageFileAssetIds != null)
            {
                var samplePageFileAssetIds = await ValidateSamplePagesAsync(request.SamplePageFileAssetIds, id);
                var oldProposalPage = await _proposalPageRepo.GetAll().Where(x => x.SeriesId == id).ToListAsync();
                var now = DateTime.UtcNow;
                //oldProposalPage.ForEach(proposalPage =>
                //{
                //    proposalPage.DeletedAt = now;
                //    _proposalPageRepo.Update(proposalPage);
                //});
                var activePages = oldProposalPage
                    .Where(x => x.DeletedAt == null)
                    .ToList();

                var temporaryStart = oldProposalPage.Count == 0
                    ? 1000
                    : oldProposalPage.Max(x => x.PageNo) + 1000;

                for (var index = 0; index < activePages.Count; index++)
                {
                    activePages[index].PageNo = temporaryStart + index;
                    _proposalPageRepo.Update(activePages[index]);
                }

                await _seriesRepo.SaveChangeAsync();

                foreach (var existingPage in oldProposalPage)
                {
                    if (existingPage.DeletedAt == null &&
                        !samplePageFileAssetIds.Contains(existingPage.PreviewFileAssetId))
                    {
                        existingPage.DeletedAt = now;
                        _proposalPageRepo.Update(existingPage);
                    }
                }

                //var proposalPage = samplePageFileAssetIds.Select((fileAssetId, index) => new ProposalPage
                //{
                //    SeriesId = series.SeriesId,
                //    PageNo = index + 1,
                //    PreviewFileAssetId = fileAssetId,
                //    CreatedAt = DateTime.UtcNow
                //});
                //await _proposalPageRepo.AddRangeAsync(proposalPage);

                for (var index = 0; index < samplePageFileAssetIds.Count; index++)
                {
                    var fileAssetId = samplePageFileAssetIds[index];

                    var existingPage = oldProposalPage.FirstOrDefault(x =>
                        x.PreviewFileAssetId == fileAssetId);

                    if (existingPage is not null)
                    {
                        existingPage.PageNo = index + 1;
                        existingPage.DeletedAt = null;

                        _proposalPageRepo.Update(existingPage);
                    }
                    else
                    {
                        await _proposalPageRepo.AddAsync(new ProposalPage
                        {
                            SeriesId = series.SeriesId,
                            PageNo = index + 1,
                            PreviewFileAssetId = fileAssetId,
                            CreatedAt = now
                        });
                    }
                }
            }

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
            SeriesId = s.SeriesId,
            MangakaId = s.MangakaId,
            MangakaName = s.Mangaka.DisplayName,
            Title = s.Title,
            Synopsis = s.Synopsis,
            PublicationType = s.PublicationType,
            Status = s.Status.ToString(),
            RankingScore = s.RankingScore,
            CreatedAt = s.CreatedAt,
            SubmittedAt = s.SubmittedAt,
            RejectReason = s.RejectReason,
            Genres = s.SeriesGenres.Where(sg => sg.Genre.DeletedAt == null).Select(sg => sg.Genre.Title).ToList(),
            CoverImageFileAssetId = s.CoverImageFileAssetId,
            SourceZipFileAssetId = s.SourceZipFileAssetId
        };

        private static string ValidateTitle(string? title)
        {
            var trimmed = title?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new ArgumentException("Title is required.");
            if (trimmed.Length > 100)
                throw new ArgumentException("Title must be 100 characters or fewer.");

            return trimmed;
        }

        private static string ValidateSynopsis(string? synopsis)
        {
            var trimmed = synopsis?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new ArgumentException("Synopsis is required.");
            if (trimmed.Length < 100 || trimmed.Length > 2000)
                throw new ArgumentException("Synopsis must be between 100 and 2000 characters.");

            return trimmed;
        }

        private static string ValidatePublicationType(string? publicationType)
        {
            var trimmed = publicationType?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new ArgumentException("Publication type is required.");
            var canonicalPublicationType = ValidPublicationTypes
                .FirstOrDefault(type => type.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
            if (canonicalPublicationType is null)
                throw new ArgumentException("Publication type must be Weekly, Monthly, or One-shot.");

            return canonicalPublicationType;
        }

        private async Task<List<Guid>> ValidateGenresAsync(IEnumerable<Guid>? genreIds)
        {
            var distinctGenreIds = genreIds?.Where(id => id != Guid.Empty).Distinct().ToList() ?? new List<Guid>();
            if (distinctGenreIds.Count == 0)
                throw new ArgumentException("At least one valid genre is required.");

            var existingCount = await _genreRepo.GetAll()
                .CountAsync(g => distinctGenreIds.Contains(g.GenreId) && g.DeletedAt == null);
            if (existingCount != distinctGenreIds.Count)
                throw new ArgumentException("Every genre must exist and not be deleted.");

            return distinctGenreIds;
        }

        //private async Task<List<Guid>> ValidateSamplePagesAsync(IEnumerable<Guid>? samplePageFileAssetIds)
        //{
        //    var distinctFileAssetIds = samplePageFileAssetIds?.Where(id => id != Guid.Empty).Distinct().ToList() ?? new List<Guid>();
        //    //if (distinctFileAssetIds.Count < MinimumSamplePageCount)
        //    //    throw new ArgumentException("At least 5 sample pages are required.");

        //    var existingFileAssets = await _fileAssetRepo.GetAll()
        //        .Where(f => distinctFileAssetIds.Contains(f.FileAssetId) && f.DeletedAt == null)
        //        .ToListAsync();
        //    if (existingFileAssets.Count != distinctFileAssetIds.Count)
        //        throw new ArgumentException("Every sample page file asset must exist and not be deleted.");
        //    if (existingFileAssets.Any(f => !IsSamplePageFile(f)))
        //        throw new ArgumentException("Every sample page file asset must be an image file uploaded as jpg, jpeg, png, gif, or webp.");

        //    var alreadyUsed = await _proposalPageRepo.GetAll()
        //        .AnyAsync(p => distinctFileAssetIds.Contains(p.PreviewFileAssetId) && p.DeletedAt == null);
        //    if (alreadyUsed)
        //        throw new InvalidOperationException("One or more sample page file assets are already used by another proposal page.");

        //    return distinctFileAssetIds;
        //}

        private async Task<List<Guid>> ValidateSamplePagesAsync(
            IEnumerable<Guid>? samplePageFileAssetIds,
            Guid? currentSeriesId = null)
        {
            var fileAssetIds = samplePageFileAssetIds?.ToList()
                ?? new List<Guid>();

            if (fileAssetIds.Count == 0)
            {
                throw new ArgumentException(
                    "At least one sample page is required.");
            }

            // Nếu cần tối thiểu 5 trang:
            // if (fileAssetIds.Count < MinimumSamplePageCount)
            // {
            //     throw new ArgumentException(
            //         "At least 5 sample pages are required.");
            // }

            if (fileAssetIds.Any(id => id == Guid.Empty))
            {
                throw new ArgumentException(
                    "Sample page file asset ID must not be empty.");
            }

            if (fileAssetIds.Count != fileAssetIds.Distinct().Count())
            {
                throw new ArgumentException(
                    "Sample page file asset IDs must not be duplicated.");
            }

            var existingFileAssets = await _fileAssetRepo.GetAll()
                .Where(fileAsset =>
                    fileAssetIds.Contains(fileAsset.FileAssetId) &&
                    fileAsset.DeletedAt == null)
                .ToListAsync();

            if (existingFileAssets.Count != fileAssetIds.Count)
            {
                throw new ArgumentException(
                    "Every sample page file asset must exist and not be deleted.");
            }

            if (existingFileAssets.Any(fileAsset => !IsSamplePageFile(fileAsset)))
            {
                throw new ArgumentException(
                    "Every sample page file asset must be an image file uploaded as jpg, jpeg, png, gif, or webp.");
            }

            var alreadyUsedByAnotherSeries = await _proposalPageRepo.GetAll()
                .AnyAsync(proposalPage =>
                    fileAssetIds.Contains(proposalPage.PreviewFileAssetId) &&
                    proposalPage.DeletedAt == null &&
                    (!currentSeriesId.HasValue ||
                     proposalPage.SeriesId != currentSeriesId.Value));

            if (alreadyUsedByAnotherSeries)
            {
                throw new InvalidOperationException(
                    "One or more sample page file assets are already used by another series.");
            }

            return fileAssetIds;
        }

        private async Task<Guid?> ValidateCoverImageAsync(
            Guid? coverImageFileAssetId,
            Guid? currentSeriesId = null)
        {
            // Cover image không bắt buộc
            if (coverImageFileAssetId == null)
                return null;

            var coverImage = await _fileAssetRepo.GetAll()
                .FirstOrDefaultAsync(f =>
                    f.FileAssetId == coverImageFileAssetId.Value &&
                    f.DeletedAt == null);

            if (coverImage == null)
            {
                throw new ArgumentException(
                    "The cover image file asset must exist and not be deleted.");
            }

            if (!IsSamplePageFile(coverImage))
            {
                throw new ArgumentException(
                    "The cover image must be an image file uploaded as jpg, jpeg, png, gif, or webp.");
            }

            var alreadyUsed = await _seriesRepo.GetAll()
                .AnyAsync(s =>
                    s.CoverImageFileAssetId == coverImageFileAssetId.Value &&
                    s.DeletedAt == null &&
                    (!currentSeriesId.HasValue ||
                     s.SeriesId != currentSeriesId.Value));

            if (alreadyUsed)
            {
                throw new InvalidOperationException(
                    "The cover image file asset is already used by another series.");
            }

            return coverImageFileAssetId.Value;
        }

        //private async Task ValidateSourceZipAsync(Guid? sourceZipFileAssetId)
        //{
        //    if (!sourceZipFileAssetId.HasValue)
        //        return;

        //    var sourceZip = await _fileAssetRepo.GetAll()
        //        .FirstOrDefaultAsync(f => f.FileAssetId == sourceZipFileAssetId.Value && f.DeletedAt == null)
        //        ?? throw new ArgumentException("Source file asset must exist and not be deleted.");

        //    if (!ValidSourceFileExtensions.Contains(sourceZip.Extension)
        //        || !ValidSourceFileMimeTypes.Contains(NormalizeMimeType(sourceZip.MimeType)))
        //        throw new ArgumentException("Source file asset must be a zip, rar, psd, clip, or ai file.");
        //}

        private async Task ValidateSourceZipAsync(
            Guid? sourceZipFileAssetId,
            Guid? currentSeriesId = null)
        {
            // Source file không bắt buộc
            if (!sourceZipFileAssetId.HasValue)
                return;

            var sourceZipId = sourceZipFileAssetId.Value;

            // Kiểm tra file tồn tại và chưa bị xóa mềm
            var sourceZip = await _fileAssetRepo.GetAll()
                .FirstOrDefaultAsync(f =>
                    f.FileAssetId == sourceZipId &&
                    f.DeletedAt == null)
                ?? throw new ArgumentException(
                    "Source file asset must exist and not be deleted.");

            // Kiểm tra extension và MIME type
            var validExtension = ValidSourceFileExtensions.Any(extension =>
                extension.Equals(
                    sourceZip.Extension,
                    StringComparison.OrdinalIgnoreCase));

            var normalizedMimeType = NormalizeMimeType(sourceZip.MimeType);

            var validMimeType = ValidSourceFileMimeTypes.Any(mimeType =>
                mimeType.Equals(
                    normalizedMimeType,
                    StringComparison.OrdinalIgnoreCase));

            if (!validExtension || !validMimeType)
            {
                throw new ArgumentException(
                    "Source file asset must be a zip, rar, psd, clip, or ai file.");
            }

            // Kiểm tra source file có đang được series khác sử dụng không
            var alreadyUsed = await _seriesRepo.GetAll()
                .AnyAsync(s =>
                    s.SourceZipFileAssetId == sourceZipId &&
                    s.DeletedAt == null &&
                    (!currentSeriesId.HasValue ||
                     s.SeriesId != currentSeriesId.Value));

            if (alreadyUsed)
            {
                throw new InvalidOperationException(
                    "The source file asset is already used by another series.");
            }
        }

        private static bool IsSamplePageFile(FileAsset fileAsset)
            => ValidSamplePageExtensions.Contains(fileAsset.Extension)
                && ValidSamplePageMimeTypes.Contains(NormalizeMimeType(fileAsset.MimeType));

        private static string NormalizeMimeType(string? mimeType)
            => (mimeType ?? string.Empty).Split(';', 2)[0].Trim().ToLowerInvariant();

        private async Task EnsureTitleDoesNotMatchActiveSeriesAsync(string title, Guid? seriesIdToExclude = null)
        {
            var normalizedTitle = title.ToLower();
            var activeTitleExists = await _seriesRepo.GetAll()
                .AnyAsync(s => s.DeletedAt == null
                    && s.Status == SeriesStatus.Active
                    && (!seriesIdToExclude.HasValue || s.SeriesId != seriesIdToExclude.Value)
                    && s.Title.ToLower() == normalizedTitle);
            if (activeTitleExists)
                throw new InvalidOperationException("Proposal title cannot match an active series title.");
        }
    }
}
