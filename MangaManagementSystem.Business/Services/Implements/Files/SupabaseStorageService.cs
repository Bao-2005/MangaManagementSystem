using MangaManagementSystem.Business.Services.Interfaces.Files;
using Microsoft.Extensions.Configuration;

namespace MangaManagementSystem.Business.Services.Implements.Files
{
    /// <summary>
    /// Stores files in Supabase Storage buckets.
    /// Bucket names are resolved from the <c>Supabase:Storage:Buckets</c> config dictionary
    /// keyed by <c>FileUploadCategory</c> name (e.g. "ProposalSamplePage").
    /// Falls back to <c>Supabase:Storage:DefaultBucket</c> when a category is not mapped.
    /// </summary>
    public class SupabaseStorageService : IStorageService
    {
        private readonly Supabase.Client _client;
        private readonly IReadOnlyDictionary<string, string> _bucketMap;
        private readonly string _defaultBucket;

        public SupabaseStorageService(IConfiguration configuration, Supabase.Client client)
        {
            _client = client;
            _defaultBucket = configuration["Supabase:Storage:DefaultBucket"] ?? "generic-uploads";

            var section = configuration.GetSection("Supabase:Storage:Buckets");
            _bucketMap = section.GetChildren()
                .ToDictionary(
                    c => c.Key,
                    c => c.Value ?? _defaultBucket,
                    StringComparer.OrdinalIgnoreCase);
        }

        public async Task<StoredFileResult> UploadAsync(
            string category,
            string originalFileName,
            string extension,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            var bucketName = ResolveBucket(category);
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var now = DateTime.UtcNow;
            var objectPath = $"{NormalizeSegment(category)}/{now:yyyy}/{now:MM}/{now:dd}/{storedFileName}";

            if (content.CanSeek)
            {
                content.Position = 0;
            }

            using var ms = new MemoryStream();
            await content.CopyToAsync(ms, cancellationToken);
            var bytes = ms.ToArray();

            await _client.Storage
                .From(bucketName)
                .Upload(bytes, objectPath, new Supabase.Storage.FileOptions
                {
                    Upsert = false
                });

            var publicUrl = _client.Storage
                .From(bucketName)
                .GetPublicUrl(objectPath);

            return new StoredFileResult
            {
                BucketName = bucketName,
                ObjectPath = objectPath,
                StoredFileName = storedFileName,
                PublicUrl = publicUrl
            };
        }

        public async Task DeleteAsync(string objectPath, CancellationToken cancellationToken = default)
        {
            // objectPath alone is not enough — we need the bucket.
            // We scan all configured buckets to try deleting; Supabase returns no error for missing objects.
            var allBuckets = _bucketMap.Values.Distinct().ToList();
            if (!allBuckets.Contains(_defaultBucket, StringComparer.OrdinalIgnoreCase))
            {
                allBuckets.Add(_defaultBucket);
            }

            foreach (var bucket in allBuckets)
            {
                try
                {
                    await _client.Storage
                        .From(bucket)
                        .Remove(new List<string> { objectPath });
                }
                catch
                {
                    // Ignore per-bucket errors; the object may not exist in that bucket.
                }
            }
        }

        private string ResolveBucket(string category)
            => _bucketMap.TryGetValue(category, out var bucket) ? bucket : _defaultBucket;

        private static string NormalizeSegment(string value)
            => string.Join("-", value.ToLowerInvariant()
                .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
    }
}
