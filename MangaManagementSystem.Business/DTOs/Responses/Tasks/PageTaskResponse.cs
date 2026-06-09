using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaManagementSystem.DataAccess.Entities.Enums;

namespace MangaManagementSystem.Business.DTOs.Responses.Tasks;

    public class PageTaskResponse
    {
        public Guid PageTaskId { get; set; }
        public Guid ChapterId { get; set; }
        public Guid ManuscriptId { get; set; }
        public Guid AssistantId { get; set; }
        public string? AssistantName { get; set; }
        public int PageStart { get; set; }
        public int PageEnd { get; set; }
        public string TaskType { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public PageTaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public IReadOnlyCollection<PageTaskSubmissionResponse> Submissions { get; set; } = Array.Empty<PageTaskSubmissionResponse>();
}

