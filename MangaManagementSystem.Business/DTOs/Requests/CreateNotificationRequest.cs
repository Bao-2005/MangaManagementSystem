using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    public class CreateNotificationRequest
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = null!;

        [MaxLength(500)]
        public string? Link { get; set; }

        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = null!;

        public List<Guid> TargetUserIds { get; set; } = new();
    }
}
