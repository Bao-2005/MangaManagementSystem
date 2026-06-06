using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    public class CreateUserAssignmentRequest
    {
        [Required]
        public Guid ToUserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AssignmentType { get; set; } = null!;
    }
}
