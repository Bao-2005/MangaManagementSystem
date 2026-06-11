using MangaManagementSystem.Business.DTOs.Requests.Files;
using MangaManagementSystem.Business.DTOs.Responses.Files;
using MangaManagementSystem.Business.Services.Interfaces.Files;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MangaManagementSystem.Business.Services.Implements.Files
{
    public class FileUploadService : IFileUploadService
    {
        private static readonly IReadOnlyDictionary<FileUploadCategory, UploadCategoryRule> Rules = new Dictionary<FileUploadCategory, UploadCategoryRule>
        {
            [FileUploadCategory.ProposalSamplePage] = new(
                10 * 1024 * 1024,
                RequireContentSignature: true,
                new Dictionary<string, string[]>
                {
                    [".jpg"] = new[] { "image/jpeg" },
                    [".jpeg"] = new[] { "image/jpeg" },
                    [".png"] = new[] { "image/png" },
                    [".gif"] = new[] { "image/gif" },
                    [".webp"] = new[] { "image/webp" }
                }),
            [FileUploadCategory.ProposalSource] = new(
                50 * 1024 * 1024,
                RequireContentSignature: true,
                new Dictionary<string, string[]>
                {
                    [".zip"] = new[] { "application/zip", "application/x-zip-compressed", "application/octet-stream" }
                }),
            [FileUploadCategory.TaskSubmission] = new(
                50 * 1024 * 1024,
                RequireContentSignature: true,
                new Dictionary<string, string[]>
                {
                    [".jpg"] = new[] { "image/jpeg" },
                    [".jpeg"] = new[] { "image/jpeg" },
                    [".png"] = new[] { "image/png" },
                    [".gif"] = new[] { "image/gif" },
                    [".webp"] = new[] { "image/webp" },
                    [".pdf"] = new[] { "application/pdf" },
                    [".zip"] = new[] { "application/zip", "application/x-zip-compressed", "application/octet-stream" }
                }),
            [FileUploadCategory.Generic] = new(
                25 * 1024 * 1024,
                RequireContentSignature: false,
                new Dictionary<string, string[]>
                {
                    [".jpg"] = new[] { "image/jpeg" },
                    [".jpeg"] = new[] { "image/jpeg" },
                    [".png"] = new[] { "image/png" },
                    [".gif"] = new[] { "image/gif" },
                    [".webp"] = new[] { "image/webp" },
                    [".pdf"] = new[] { "application/pdf" },
                    [".zip"] = new[] { "application/zip", "application/x-zip-compressed", "application/octet-stream" }
                })
        };

        private readonly IRepository<FileAsset> _fileAssetRepository;
        private readonly IStorageService _storageService;
        private readonly string _supabaseUrl;

        public FileUploadService(
            IRepository<FileAsset> fileAssetRepository,
            IStorageService storageService,
            IConfiguration configuration)
        {
            _fileAssetRepository = fileAssetRepository;
            _storageService = storageService;
            _supabaseUrl = (configuration["Supabase:Url"] ?? string.Empty).TrimEnd('/');
        }

        public async Task<FileUploadResponse> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request.Files.Count == 0)
                {
                    throw new ArgumentException("At least one file is required.");
                }

                if (request.Files.Count > FileUploadLimits.MaxFilesPerRequest)
                {
                    throw new ArgumentException($"A maximum of {FileUploadLimits.MaxFilesPerRequest} files can be uploaded per request.");
                }

                var totalUploadBytes = request.Files.Sum(file => file.Length);
                if (totalUploadBytes > FileUploadLimits.MaxTotalUploadBytes)
                {
                    throw new ArgumentException($"Total upload size exceeds {FileUploadLimits.MaxTotalUploadBytes} bytes.");
                }

                var rule = Rules.TryGetValue(request.Category, out var matchedRule)
                    ? matchedRule
                    : Rules[FileUploadCategory.Generic];

                var validationErrors = new List<string>();
                foreach (var file in request.Files)
                {
                    var error = await ValidateAsync(file, rule, cancellationToken);
                    if (error is not null)
                    {
                        validationErrors.Add(error);
                    }
                }

                if (validationErrors.Count > 0)
                {
                    var prefix = validationErrors.Count == request.Files.Count
                        ? "All files failed validation."
                        : "One or more files failed validation; no files were uploaded.";
                    throw new ArgumentException($"{prefix} {string.Join(" ", validationErrors)}");
                }

                var assets = new List<FileAsset>();
                var storedFiles = new List<StoredFileResult>();
                try
                {
                    foreach (var file in request.Files)
                    {
                        var extension = Path.GetExtension(file.OriginalFileName).ToLowerInvariant();
                        var storageResult = await _storageService.UploadAsync(
                            request.Category.ToString(),
                            file.OriginalFileName,
                            extension,
                            file.Content,
                            cancellationToken);
                        storedFiles.Add(storageResult);

                        var asset = new FileAsset
                        {
                            BucketName = storageResult.BucketName,
                            ObjectPath = storageResult.ObjectPath,
                            OriginalFileName = Path.GetFileName(file.OriginalFileName),
                            StoredFileName = storageResult.StoredFileName,
                            Extension = extension,
                            FileSizeBytes = file.Length,
                            MimeType = NormalizeMimeType(file.ContentType)
                        };

                        await _fileAssetRepository.AddAsync(asset, cancellationToken);
                        assets.Add(asset);
                    }

                    await _fileAssetRepository.SaveChangeAsync(cancellationToken);

                    return new FileUploadResponse
                    {
                        Category = request.Category.ToString(),
                        Files = assets.Select(Map).ToList()
                    };
                }
                catch
                {
                    await DeleteStoredFilesAsync(storedFiles, cancellationToken);
                    throw;
                }
            }
            finally
            {
                DisposeStreams(request.Files);
            }
        }

        public async Task<FileAssetResponse?> GetByIdAsync(Guid fileAssetId, CancellationToken cancellationToken = default)
        {
            var asset = await _fileAssetRepository.GetAll()
                .FirstOrDefaultAsync(x => x.FileAssetId == fileAssetId && x.DeletedAt == null, cancellationToken);
            return asset is null ? null : Map(asset);
        }

        private static async Task<string?> ValidateAsync(UploadFileRequest file, UploadCategoryRule rule, CancellationToken cancellationToken)
        {
            if (file.Length <= 0)
            {
                return $"{file.OriginalFileName}: file is empty.";
            }

            if (!file.Content.CanRead)
            {
                return $"{file.OriginalFileName}: file stream is not readable.";
            }

            if (!file.Content.CanSeek)
            {
                return $"{file.OriginalFileName}: file stream must be seekable.";
            }

            if (file.Length > rule.MaxFileSizeBytes)
            {
                return $"{file.OriginalFileName}: file size exceeds {rule.MaxFileSizeBytes} bytes.";
            }

            var extension = Path.GetExtension(file.OriginalFileName).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension) || !rule.AllowedMimeTypesByExtension.TryGetValue(extension, out var allowedMimeTypes))
            {
                return $"{file.OriginalFileName}: extension '{extension}' is not allowed for this upload category.";
            }

            var mimeType = NormalizeMimeType(file.ContentType);
            if (string.IsNullOrWhiteSpace(mimeType) || !allowedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase))
            {
                return $"{file.OriginalFileName}: MIME type '{file.ContentType}' is not allowed for extension '{extension}'.";
            }

            if ((rule.RequireContentSignature || extension == ".pdf") && RequiresSignatureValidation(extension))
            {
                var signature = await ReadSignatureAsync(file.Content, cancellationToken);
                if (!MatchesSignature(extension, signature))
                {
                    return $"{file.OriginalFileName}: file content does not match extension '{extension}'.";
                }
            }

            return null;
        }

        private static bool RequiresSignatureValidation(string extension)
            => extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".pdf" or ".zip";

        private static async Task<byte[]> ReadSignatureAsync(Stream content, CancellationToken cancellationToken)
        {
            var originalPosition = content.Position;
            content.Position = 0;

            var buffer = new byte[12];
            var bytesRead = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

            content.Position = originalPosition;

            return buffer.Take(bytesRead).ToArray();
        }

        private static bool MatchesSignature(string extension, byte[] signature)
            => extension switch
            {
                ".jpg" or ".jpeg" => signature.Length >= 3
                    && signature[0] == 0xFF
                    && signature[1] == 0xD8
                    && signature[2] == 0xFF,
                ".png" => signature.Length >= 8
                    && signature[0] == 0x89
                    && signature[1] == 0x50
                    && signature[2] == 0x4E
                    && signature[3] == 0x47
                    && signature[4] == 0x0D
                    && signature[5] == 0x0A
                    && signature[6] == 0x1A
                    && signature[7] == 0x0A,
                ".gif" => signature.Length >= 6
                    && signature[0] == 0x47
                    && signature[1] == 0x49
                    && signature[2] == 0x46
                    && signature[3] == 0x38
                    && (signature[4] == 0x37 || signature[4] == 0x39)
                    && signature[5] == 0x61,
                ".webp" => signature.Length >= 12
                    && signature[0] == 0x52
                    && signature[1] == 0x49
                    && signature[2] == 0x46
                    && signature[3] == 0x46
                    && signature[8] == 0x57
                    && signature[9] == 0x45
                    && signature[10] == 0x42
                    && signature[11] == 0x50,
                ".pdf" => signature.Length >= 5
                    && signature[0] == 0x25
                    && signature[1] == 0x50
                    && signature[2] == 0x44
                    && signature[3] == 0x46
                    && signature[4] == 0x2D,
                ".zip" => signature.Length >= 4
                    && signature[0] == 0x50
                    && signature[1] == 0x4B
                    && (signature[2], signature[3]) is (0x03, 0x04) or (0x05, 0x06) or (0x07, 0x08),
                _ => true
            };

        private async Task DeleteStoredFilesAsync(IEnumerable<StoredFileResult> storedFiles, CancellationToken cancellationToken)
        {
            foreach (var storedFile in storedFiles)
            {
                try
                {
                    await _storageService.DeleteAsync(storedFile.ObjectPath, cancellationToken);
                }
                catch
                {
                    // Preserve the original upload or database failure.
                }
            }
        }

        private static void DisposeStreams(IEnumerable<UploadFileRequest> files)
        {
            foreach (var file in files)
            {
                file.Content.Dispose();
            }
        }

        private static string NormalizeMimeType(string? mimeType)
            => (mimeType ?? string.Empty).Split(';', 2)[0].Trim().ToLowerInvariant();

        private FileAssetResponse Map(FileAsset asset) => new()
        {
            FileAssetId = asset.FileAssetId,
            BucketName = asset.BucketName,
            ObjectPath = asset.ObjectPath,
            OriginalFileName = asset.OriginalFileName,
            StoredFileName = asset.StoredFileName,
            Extension = asset.Extension,
            FileSizeBytes = asset.FileSizeBytes,
            MimeType = asset.MimeType,
            PublicUrl = string.IsNullOrEmpty(_supabaseUrl)
                ? null
                : $"{_supabaseUrl}/storage/v1/object/public/{asset.BucketName}/{asset.ObjectPath}"
        };

        private sealed record UploadCategoryRule(
            long MaxFileSizeBytes,
            bool RequireContentSignature,
            IReadOnlyDictionary<string, string[]> AllowedMimeTypesByExtension);
    }
}
