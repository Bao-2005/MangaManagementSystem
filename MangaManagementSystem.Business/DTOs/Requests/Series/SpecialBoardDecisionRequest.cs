using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class SpecialBoardDecisionRequest
    {
        [Required]
        [MaxLength(50)]
        public string Decision { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = null!;
    }
}
