using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Tasks
{
    public class UpdatePageTaskRequest
    {
        public Guid? AssistantId { get; set; }

        [Range(1, int.MaxValue)]
        public int? PageStart { get; set; }

        [Range(1, int.MaxValue)]
        public int? PageEnd { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? RatePerPage { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
