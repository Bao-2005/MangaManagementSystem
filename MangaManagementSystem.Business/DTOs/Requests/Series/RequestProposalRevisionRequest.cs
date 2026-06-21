using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class RequestProposalRevisionRequest
    {
        [Required]
        [MaxLength(1000)]
        public string RevisionReason { get; set; } = null!;
    }
}
