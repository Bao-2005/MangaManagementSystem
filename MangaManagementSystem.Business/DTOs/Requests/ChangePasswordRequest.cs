using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = null!;
    }
}
