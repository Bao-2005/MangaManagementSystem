using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class Escalation
    {
        public Guid EscalationId { get; set; }

        public string Type { get; set; } = null!;
        public string EntityType { get; set; } = null!;
        public Guid EntityId { get; set; }
        public Guid SeriesId { get; set; }
        public string Priority { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public string? Resolution { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? ResolvedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Series Series { get; set; } = null!;
        public User Creator { get; set; } = null!;
        public User? Resolver { get; set; }
    }
}
