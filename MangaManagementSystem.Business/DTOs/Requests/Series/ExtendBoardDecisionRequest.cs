using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class ExtendBoardDecisionRequest
    {
        [Required]
        public DateTime NewDeadline { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = null!;
    }
}
