using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    public class CreateNotificationRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = null!;

        public List<Guid> TargetUserIds { get; set; } = new();
    }
}
