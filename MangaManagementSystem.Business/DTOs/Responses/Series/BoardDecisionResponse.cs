namespace MangaManagementSystem.Business.DTOs.Responses.Series
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
        public Guid? CreatedBy { get; set; }
        public int ExtensionCount { get; set; }
        public Guid? ExtendedBy { get; set; }
        public DateTime? ExtendedAt { get; set; }
        public string? ExtensionReason { get; set; }
        public Guid? SpecialDecisionBy { get; set; }
        public DateTime? SpecialDecisionAt { get; set; }
        public string? SpecialDecisionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VoteCount { get; set; }
    }
}
