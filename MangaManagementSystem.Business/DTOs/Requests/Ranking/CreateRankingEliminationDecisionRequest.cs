using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Ranking
{
    public class CreateRankingEliminationDecisionRequest
    {
        [Required]
        public DateTime VotingDeadline { get; set; }
    }
}
