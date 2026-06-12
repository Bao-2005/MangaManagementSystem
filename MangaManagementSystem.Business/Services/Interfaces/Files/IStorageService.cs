namespace MangaManagementSystem.Business.Services.Interfaces.Files
{
    public interface IStorageService
    {
        Task<StoredFileResult> UploadAsync(
            string category,
            string originalFileName,
            string extension,
            Stream content,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(string objectPath, CancellationToken cancellationToken = default);
    }

    public class StoredFileResult
    {
        public string BucketName { get; set; } = null!;
        public string ObjectPath { get; set; } = null!;
        public string StoredFileName { get; set; } = null!;
        public string? PublicUrl { get; set; }
    }
}
