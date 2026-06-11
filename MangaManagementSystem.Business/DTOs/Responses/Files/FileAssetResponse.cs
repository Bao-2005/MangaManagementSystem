namespace MangaManagementSystem.Business.DTOs.Responses.Files
{
    public class FileUploadResponse
    {
        public string Category { get; set; } = null!;
        public IReadOnlyCollection<FileAssetResponse> Files { get; set; } = Array.Empty<FileAssetResponse>();
    }

    public class FileAssetResponse
    {
        public Guid FileAssetId { get; set; }
        public string BucketName { get; set; } = null!;
        public string ObjectPath { get; set; } = null!;
        public string OriginalFileName { get; set; } = null!;
        public string StoredFileName { get; set; } = null!;
        public string Extension { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public string MimeType { get; set; } = null!;
        public string? PublicUrl { get; set; }
    }
}
