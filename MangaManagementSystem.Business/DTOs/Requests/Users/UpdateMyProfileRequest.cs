using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Users
{
    public class UpdateMyProfileRequest
    {
        [MaxLength(100)]
        public string? UserName { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(150)]
        public string? DisplayName { get; set; }
    }
}
