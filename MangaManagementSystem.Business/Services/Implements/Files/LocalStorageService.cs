using MangaManagementSystem.Business.Services.Interfaces.Files;
using Microsoft.Extensions.Configuration;

namespace MangaManagementSystem.Business.Services.Implements.Files
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _bucketName;
        private readonly string _rootPath;

        public LocalStorageService(IConfiguration configuration)
        {
            _bucketName = configuration["FileStorage:BucketName"] ?? "local-files";
            _rootPath = configuration["FileStorage:RootPath"]
                ?? Path.Combine(Path.GetTempPath(), "MangaManagementSystem", "uploads");
        }

        public async Task<StoredFileResult> UploadAsync(
            string category,
            string originalFileName,
            string extension,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var objectPath = Path.Combine(
                    NormalizeSegment(category),
                    DateTime.UtcNow.ToString("yyyy"),
                    DateTime.UtcNow.ToString("MM"),
                    DateTime.UtcNow.ToString("dd"),
                    storedFileName)
                .Replace(Path.DirectorySeparatorChar, '/');

            var fullPath = Path.Combine(_rootPath, objectPath.Replace('/', Path.DirectorySeparatorChar));
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (content.CanSeek)
            {
                content.Position = 0;
            }

            try
            {
                await using var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                await content.CopyToAsync(fileStream, cancellationToken);
            }
            catch
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                throw;
            }

            return new StoredFileResult
            {
                BucketName = _bucketName,
                ObjectPath = objectPath,
                StoredFileName = storedFileName
            };
        }

        public Task DeleteAsync(string objectPath, CancellationToken cancellationToken = default)
        {
            var fullPath = Path.GetFullPath(Path.Combine(_rootPath, objectPath.Replace('/', Path.DirectorySeparatorChar)));
            var rootPath = Path.GetFullPath(_rootPath);
            var rootPrefix = rootPath.EndsWith(Path.DirectorySeparatorChar)
                ? rootPath
                : rootPath + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Storage object path is outside the configured storage root.");
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

        private static string NormalizeSegment(string value)
            => string.Join("-", value.ToLowerInvariant().Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
    }
}
