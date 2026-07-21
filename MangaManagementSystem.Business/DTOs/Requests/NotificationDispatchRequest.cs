using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    public class NotificationDispatchRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = null!;
    }
}
