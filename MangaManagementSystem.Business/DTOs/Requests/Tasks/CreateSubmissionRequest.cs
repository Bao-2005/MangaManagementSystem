using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Tasks
{
    public class CreateSubmissionRequest
    {
        [Required]
        public Guid PageTaskId { get; set; }

        [Required]
        public int VersionNo { get; set; }

        [Required]
        public Guid SubmittedFileAssetId { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}
