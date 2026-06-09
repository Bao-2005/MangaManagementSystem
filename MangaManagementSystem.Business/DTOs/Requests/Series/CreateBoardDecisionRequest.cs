using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class CreateBoardDecisionRequest
    {
        [Required]
        public Guid SeriesId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DecisionType { get; set; } = null!;

        [Required]
        public DateTime VotingDeadline { get; set; }
    }
}
