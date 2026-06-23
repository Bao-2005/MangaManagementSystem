using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Tasks
{
    public class AttachPageTaskReferenceFilesRequest
    {
        [Required]
        public IReadOnlyCollection<Guid> FileAssetIds { get; set; } = Array.Empty<Guid>();
    }
}
