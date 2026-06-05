using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class BoardVote
    {
        public Guid BoardVoteId { get; set; }
        public Guid BoardDecisionId { get; set; }
        public Guid VoterId { get; set; }

        public bool VoteValue { get; set; }
        public DateTime VotedAt { get; set; }
        public string? Comment { get; set; }
        public DateTime? DeletedAt { get; set; }

        public BoardDecision BoardDecision { get; set; } = null!;
        public User Voter { get; set; } = null!;
    }
}
