using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class BoardDecision
    {
        public Guid BoardDecisionId { get; set; }
        public Guid SeriesId { get; set; }

        public string DecisionType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Result { get; set; }
        public DateTime VotingDeadline { get; set; }
        public DateTime? FinalizedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Series Series { get; set; } = null!;
        public User? Creator { get; set; }
        public ICollection<BoardVote> BoardVotes { get; set; } = new List<BoardVote>();
    }
}
