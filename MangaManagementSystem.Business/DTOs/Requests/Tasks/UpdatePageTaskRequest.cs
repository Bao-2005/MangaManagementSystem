using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Tasks
{
    public class UpdatePageTaskRequest
    {
        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
