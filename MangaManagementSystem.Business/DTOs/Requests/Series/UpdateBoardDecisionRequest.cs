using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class UpdateBoardDecisionRequest
    {
        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(50)]
        public string? Result { get; set; }

        public DateTime? FinalizedAt { get; set; }
    }
}
