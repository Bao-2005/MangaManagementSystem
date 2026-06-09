using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class RejectProposalRequest
    {
        [Required]
        [MaxLength(1000)]
        public string RejectReason { get; set; } = null!;
    }
}
