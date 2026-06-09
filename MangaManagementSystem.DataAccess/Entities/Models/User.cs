using MangaManagementSystem.DataAccess.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Role Role { get; set; } = null!;

        public ICollection<Series> Series { get; set; } = new List<Series>();
        public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
        public ICollection<UserAssignment> AssignmentsFromUser { get; set; } = new List<UserAssignment>();
        public ICollection<UserAssignment> AssignmentsToUser { get; set; } = new List<UserAssignment>();
        public ICollection<BoardDecision> CreatedBoardDecisions { get; set; } = new List<BoardDecision>();
        public ICollection<BoardVote> BoardVotes { get; set; } = new List<BoardVote>();
        public ICollection<Manuscript> ReviewedManuscripts { get; set; } = new List<Manuscript>();
        public ICollection<PageTask> AssistantPageTasks { get; set; } = new List<PageTask>();
        public ICollection<PageTask> AssignedPageTasks { get; set; } = new List<PageTask>();
        public ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
        public ICollection<VoteRecord> ConfirmedVoteRecords { get; set; } = new List<VoteRecord>();
        public ICollection<Escalation> CreatedEscalations { get; set; } = new List<Escalation>();
        public ICollection<Escalation> ResolvedEscalations { get; set; } = new List<Escalation>();
    }
}
