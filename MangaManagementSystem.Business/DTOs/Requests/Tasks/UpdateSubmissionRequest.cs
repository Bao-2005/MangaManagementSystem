using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Tasks
{
    public class UpdateSubmissionRequest
    {
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = null!;

        [MaxLength(1000)]
        public string? RejectReason { get; set; }
    }
}
