namespace MangaManagementSystem.Business.DTOs.Responses.Series
{
    public class BoardDecisionSummaryResponse
    {
        public Guid BoardDecisionId { get; set; }
        public Guid SeriesId { get; set; }
        public string DecisionType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Result { get; set; }
        public DateTime VotingDeadline { get; set; }
        public DateTime? FinalizedAt { get; set; }
        public int VoteCount { get; set; }
        public int ApproveCount { get; set; }
        public int RejectCount { get; set; }
        public int RequiredQuorum { get; set; }
        public bool HasQuorum { get; set; }
        public bool IsDeadlinePassed { get; set; }
    }
}
