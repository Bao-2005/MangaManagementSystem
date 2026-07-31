using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Users
{
    public class CreateUserAssignmentRequest
    {
        [Required]
        public Guid FromUserId { get; set; }
        [Required]
        public Guid ToUserId { get; set; }
    }
}
