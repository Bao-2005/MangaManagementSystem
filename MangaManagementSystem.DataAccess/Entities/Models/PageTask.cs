using MangaManagementSystem.DataAccess.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class PageTask
    {
        public Guid PageTaskId { get; set; }

        public Guid ChapterId { get; set; }

        public Guid? ManuscriptId { get; set; }

        public Guid AssistantId { get; set; }

        public int PageStart { get; set; }

        public int PageEnd { get; set; }

        public string? TaskType { get; set; }

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public PageTaskStatus Status { get; set; } = PageTaskStatus.Assigned;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Chapter Chapter { get; set; } = null!;

        public Manuscript? Manuscript { get; set; }

        public User Assistant { get; set; } = null!;

        public ICollection<PageTaskSubmission> Submissions { get; set; } = new List<PageTaskSubmission>();
        public ICollection<PageTaskReferenceFile> ReferenceFiles { get; set; } = new List<PageTaskReferenceFile>();
    }
}
