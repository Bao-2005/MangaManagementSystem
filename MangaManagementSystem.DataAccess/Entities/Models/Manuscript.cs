using MangaManagementSystem.DataAccess.Entities.Enums;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class Manuscript
    {
        public Guid ManuscriptId { get; set; }
        public Guid ChapterId { get; set; }

        public int VersionNo { get; set; }
        public string FileUrl { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Feedback { get; set; }
        public int RevisionCount { get; set; }
        public DateTime SubmittedAt { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Chapter Chapter { get; set; } = null!;
        public User? Reviewer { get; set; }
        public ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
        public ICollection<PageTask> PageTasks { get; set; } = new List<PageTask>();
    }
}
