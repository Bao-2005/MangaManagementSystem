using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Users
{
    public class AdminUpdateUserRequest
    {
        [MaxLength(100)]
        public string? UserName { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(150)]
        public string? DisplayName { get; set; }

        //public Guid? RoleId { get; set; }

        [MinLength(6)]
        public string? NewPassword { get; set; }
    }
}
