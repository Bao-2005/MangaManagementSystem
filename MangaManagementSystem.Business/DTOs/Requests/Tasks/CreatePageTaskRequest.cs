using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Tasks
{ 
    public class CreatePageTaskRequest
    {
        [Required]
        public Guid ChapterId { get; set; }

        [Required]
        public Guid AssistantId { get; set; }

        [Range(1, int.MaxValue)]
        public int PageStart { get; set; }

        [Range(1, int.MaxValue)]
        public int PageEnd { get; set; }

        [Required]
        [MaxLength(50)]
        public string TaskType { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
