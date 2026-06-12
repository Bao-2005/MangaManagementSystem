using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Manuscripts
{
    public class CreateManuscriptRequest
    {
        [Required]
        public Guid ChapterId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string FileUrl { get; set; } = null!;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
