namespace MangaManagementSystem.Business.DTOs.Responses
{
    public class BoardDecisionResponse
    {
        public Guid BoardDecisionId { get; set; }
        public Guid SeriesId { get; set; }
        public string DecisionType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Result { get; set; }
        public DateTime VotingDeadline { get; set; }
        public DateTime? FinalizedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VoteCount { get; set; }
    }
}
