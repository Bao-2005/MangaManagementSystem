using MangaManagementSystem.Business.DTOs.Requests.Chapters;
using MangaManagementSystem.Business.DTOs.Responses.Chapters;
using MangaManagementSystem.Business.DTOs.Responses.Files;
using MangaManagementSystem.Business.Services.Interfaces.Chapters;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SeriesEntity = MangaManagementSystem.DataAccess.Entities.Models.Series;

namespace MangaManagementSystem.Business.Services.Implements.Chapters
{
    public class ChapterService : IChapterService
    {
        private readonly IRepository<Chapter> _repo;
        private readonly IRepository<SeriesEntity> _seriesRepo;
        private readonly IRepository<Manuscript> _manuscriptRepo;
        private readonly IRepository<PageTask> _pageTaskRepo;
        private readonly IRepository<FileAsset> _fileAssetRepo;
        private readonly IRepository<ChapterReferenceFile> _chapterReferenceFileRepo;
        private readonly string _supabaseUrl;

        public ChapterService(
            IRepository<Chapter> repo,
            IRepository<SeriesEntity> seriesRepo,
            IRepository<Manuscript> manuscriptRepo,
            IRepository<PageTask> pageTaskRepo,
            IRepository<FileAsset> fileAssetRepo,
            IRepository<ChapterReferenceFile> chapterReferenceFileRepo,
            IConfiguration configuration)
        {
            _repo = repo;
            _seriesRepo = seriesRepo;
            _manuscriptRepo = manuscriptRepo;
            _pageTaskRepo = pageTaskRepo;
            _fileAssetRepo = fileAssetRepo;
            _chapterReferenceFileRepo = chapterReferenceFileRepo;
            _supabaseUrl = (configuration["Supabase:Url"] ?? string.Empty).TrimEnd('/');
        }

        public async Task<IEnumerable<ChapterResponse>> GetAllAsync()
        {
            var chapters = await ChapterQuery()
                .Where(c => c.DeletedAt == null)
                .ToListAsync();
            return chapters.Select(Map);
        }

        public async Task<IEnumerable<ChapterResponse>> GetBySeriesAsync(Guid seriesId)
        {
            var chapters = await ChapterQuery()
                .Where(c => c.SeriesId == seriesId && c.DeletedAt == null)
                .OrderBy(c => c.ChapterNo)
                .ToListAsync();
            return chapters.Select(Map);
        }

        public async Task<ChapterResponse> GetByIdAsync(Guid id)
        {
            var c = await ChapterQuery().FirstOrDefaultAsync(x => x.ChapterId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Chapter not found.");
            return Map(c);
        }

        public async Task<ChapterResponse> CreateAsync(Guid mangakaId, CreateChapterRequest request)
        {
            var series = await _seriesRepo.GetAll()
                .FirstOrDefaultAsync(s => s.SeriesId == request.SeriesId && s.DeletedAt == null)
                ?? throw new KeyNotFoundException("Series not found.");

            if (series.MangakaId != mangakaId)
                throw new UnauthorizedAccessException("Only the owner Mangaka can create chapters for this series.");

            if (series.Status != SeriesStatus.Approved && series.Status != SeriesStatus.Active)
                throw new InvalidOperationException("Chapters can only be created for approved or active series.");

            if (request.ChapterNo <= 0)
                throw new ArgumentException("Chapter number must be greater than 0.");

            if (request.TotalPages <= 0)
                throw new ArgumentException("Total pages must be greater than 0.");

            var title = request.Title?.Trim();
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.");

            var hasSameChapterNo = await _repo.GetAll()
                .AnyAsync(c => c.SeriesId == request.SeriesId
                    && c.ChapterNo == request.ChapterNo
                    && c.DeletedAt == null);
            if (hasSameChapterNo)
                throw new InvalidOperationException("Chapter number already exists in this series.");

            var now = DateTime.UtcNow;
            var publicationDate = request.PublicationDate;
            if (publicationDate.HasValue && publicationDate.Value.Date < now.Date)
                throw new ArgumentException("Publication date cannot be in the past.");

            // BR-42: deadline = publicationDate - 14 days if not provided.
            var deadline = request.SubmissionDeadline
                           ?? (publicationDate.HasValue ? publicationDate.Value.AddDays(-14) : null);

            if (publicationDate.HasValue && deadline.HasValue)
            {
                if (deadline.Value.Date > publicationDate.Value.Date)
                    throw new ArgumentException("Submission deadline cannot be after publication date.");

                if (deadline.Value.Date < now.Date.AddDays(3))
                    throw new ArgumentException("Submission deadline must be at least 3 days after chapter creation.");
            }

            var chapter = new Chapter
            {
                SeriesId = request.SeriesId,
                ChapterNo = request.ChapterNo,
                Title = title,
                TotalPages = request.TotalPages,
                PublicationDate = publicationDate,
                SubmissionDeadline = deadline,
                Status = "Draft",
                CreatedAt = now
            };
            await _repo.AddAsync(chapter);

            var fileAssetIds = NormalizeFileAssetIds(request.ReferenceFileAssetIds);
            if (fileAssetIds.Count > 0)
            {
                await EnsureFileAssetsExistAsync(fileAssetIds);

                foreach (var fileAssetId in fileAssetIds)
                {
                    chapter.ReferenceFiles.Add(new ChapterReferenceFile
                    {
                        FileAssetId = fileAssetId,
                        CreatedAt = now
                    });
                }
            }

            await _repo.SaveChangeAsync();
            var createdChapter = await ChapterQuery().FirstAsync(x => x.ChapterId == chapter.ChapterId);
            return Map(createdChapter);
        }

        public async Task<ChapterResponse> AddReferenceFilesAsync(Guid mangakaId, Guid chapterId, AttachChapterReferenceFilesRequest request)
        {
            var chapter = await _repo.GetAll()
                .Include(c => c.Series)
                .FirstOrDefaultAsync(c => c.ChapterId == chapterId && c.DeletedAt == null)
                ?? throw new KeyNotFoundException("Chapter not found.");

            if (chapter.Series.MangakaId != mangakaId)
                throw new UnauthorizedAccessException("Only the owner Mangaka can attach reference files to this chapter.");

            var fileAssetIds = NormalizeFileAssetIds(request.FileAssetIds);
            if (fileAssetIds.Count == 0)
                throw new ArgumentException("At least one file asset is required.");

            await EnsureFileAssetsExistAsync(fileAssetIds);

            var existingFileAssetIds = await _chapterReferenceFileRepo.GetAll()
                .Where(x => x.ChapterId == chapterId
                    && x.DeletedAt == null
                    && fileAssetIds.Contains(x.FileAssetId))
                .Select(x => x.FileAssetId)
                .ToListAsync();

            var existingSet = existingFileAssetIds.ToHashSet();
            var newReferenceFiles = fileAssetIds
                .Where(fileAssetId => !existingSet.Contains(fileAssetId))
                .Select(fileAssetId => new ChapterReferenceFile
                {
                    ChapterId = chapterId,
                    FileAssetId = fileAssetId,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            if (newReferenceFiles.Count > 0)
            {
                await _chapterReferenceFileRepo.AddRangeAsync(newReferenceFiles);
                await _chapterReferenceFileRepo.SaveChangeAsync();
            }

            var updatedChapter = await ChapterQuery().FirstAsync(x => x.ChapterId == chapterId);
            return Map(updatedChapter);
        }

        public async Task<ChapterResponse> UpdateAsync(Guid id, UpdateChapterRequest request)
        {
            var c = await _repo.GetAll().FirstOrDefaultAsync(x => x.ChapterId == id && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Chapter not found.");
            if (request.Title != null) c.Title = request.Title;
            if (request.TotalPages.HasValue) c.TotalPages = request.TotalPages.Value;
            if (request.PublicationDate.HasValue) c.PublicationDate = request.PublicationDate;
            if (request.SubmissionDeadline.HasValue) c.SubmissionDeadline = request.SubmissionDeadline;
            if (request.Status != null) c.Status = request.Status;
            _repo.Update(c);
            await _repo.SaveChangeAsync();
            var updatedChapter = await ChapterQuery().FirstAsync(x => x.ChapterId == id);
            return Map(updatedChapter);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var chapter = await _repo.GetAll()
                .Include(c => c.Manuscripts)
                .Include(c => c.PageTasks)
                .Include(c => c.ReferenceFiles)
                .FirstOrDefaultAsync(x => x.ChapterId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Chapter not found.");

            var now = DateTime.UtcNow;
            chapter.DeletedAt = now;
            foreach (var m in chapter.Manuscripts.Where(m => m.DeletedAt == null)) m.DeletedAt = now;
            foreach (var pt in chapter.PageTasks.Where(pt => pt.DeletedAt == null)) pt.DeletedAt = now;
            foreach (var rf in chapter.ReferenceFiles.Where(rf => rf.DeletedAt == null)) rf.DeletedAt = now;
            _repo.Update(chapter);
            await _repo.SaveChangeAsync();
        }

        private IQueryable<Chapter> ChapterQuery()
            => _repo.GetAll()
                .Include(c => c.ReferenceFiles.Where(rf => rf.DeletedAt == null))
                .ThenInclude(rf => rf.FileAsset);

        private async Task EnsureFileAssetsExistAsync(IReadOnlyCollection<Guid> fileAssetIds)
        {
            var existingFileAssetIds = await _fileAssetRepo.GetAll()
                .Where(fileAsset => fileAssetIds.Contains(fileAsset.FileAssetId) && fileAsset.DeletedAt == null)
                .Select(fileAsset => fileAsset.FileAssetId)
                .ToListAsync();

            if (existingFileAssetIds.Count != fileAssetIds.Count)
            {
                var missingFileAssetIds = fileAssetIds.Except(existingFileAssetIds).ToList();
                throw new KeyNotFoundException($"File asset not found: {string.Join(", ", missingFileAssetIds)}.");
            }
        }

        private static IReadOnlyCollection<Guid> NormalizeFileAssetIds(IEnumerable<Guid>? fileAssetIds)
            => (fileAssetIds ?? Array.Empty<Guid>())
                .Where(fileAssetId => fileAssetId != Guid.Empty)
                .Distinct()
                .ToList();

        private ChapterResponse Map(Chapter c) => new()
        {
            ChapterId = c.ChapterId, SeriesId = c.SeriesId, ChapterNo = c.ChapterNo, Title = c.Title,
            TotalPages = c.TotalPages, Status = c.Status, PublicationDate = c.PublicationDate,
            SubmissionDeadline = c.SubmissionDeadline, CreatedAt = c.CreatedAt,
            ReferenceFiles = c.ReferenceFiles
                .Where(rf => rf.DeletedAt == null && rf.FileAsset.DeletedAt == null)
                .OrderBy(rf => rf.CreatedAt)
                .Select(rf => MapFileAsset(rf.FileAsset))
                .ToList()
        };

        private FileAssetResponse MapFileAsset(FileAsset fileAsset) => new()
        {
            FileAssetId = fileAsset.FileAssetId,
            BucketName = fileAsset.BucketName,
            ObjectPath = fileAsset.ObjectPath,
            OriginalFileName = fileAsset.OriginalFileName,
            StoredFileName = fileAsset.StoredFileName,
            Extension = fileAsset.Extension,
            FileSizeBytes = fileAsset.FileSizeBytes,
            MimeType = fileAsset.MimeType,
            PublicUrl = string.IsNullOrEmpty(_supabaseUrl)
                ? null
                : $"{_supabaseUrl}/storage/v1/object/public/{fileAsset.BucketName}/{fileAsset.ObjectPath}"
        };
    }
}
