namespace MangaManagementSystem.Business.DTOs.Responses.Series
{
    public class BoardVoteResponse
    {
        public Guid BoardVoteId { get; set; }
        public Guid BoardDecisionId { get; set; }
        public string DecisionType { get; set; } = null!;
        public Guid VoterId { get; set; }
        public string VoterName { get; set; } = null!;
        public bool VoteValue { get; set; }
        public DateTime VotedAt { get; set; }
        public string? Comment { get; set; }
    }
}
