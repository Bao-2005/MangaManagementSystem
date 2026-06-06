namespace MangaManagementSystem.Business.DTOs.Responses
{
    public class PageTaskResponse
    {
        public Guid PageTaskId { get; set; }
        public Guid ChapterId { get; set; }
        public Guid ManuscriptId { get; set; }
        public Guid AssistantId { get; set; }
        public string AssistantName { get; set; } = null!;
        public int PageStart { get; set; }
        public int PageEnd { get; set; }
        public string TaskType { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
