using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Chapters
{
    public class AttachChapterReferenceFilesRequest
    {
        [Required]
        public IReadOnlyCollection<Guid> FileAssetIds { get; set; } = Array.Empty<Guid>();
    }
}
