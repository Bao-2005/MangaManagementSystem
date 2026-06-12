namespace MangaManagementSystem.Business.DTOs.Requests.Files
{
    public class FileUploadRequest
    {
        public FileUploadCategory Category { get; set; } = FileUploadCategory.Generic;
        public IReadOnlyCollection<UploadFileRequest> Files { get; set; } = Array.Empty<UploadFileRequest>();
    }

    public class UploadFileRequest
    {
        public string OriginalFileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long Length { get; set; }
        public Stream Content { get; set; } = null!;
    }
}
