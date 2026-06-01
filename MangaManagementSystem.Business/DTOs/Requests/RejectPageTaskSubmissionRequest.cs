using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    public class RejectPageTaskSubmissionRequest
    {
        [Required]
        [MaxLength(1000)]
        public string RejectReason { get; set; } = null!;
    }
}
