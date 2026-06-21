using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class CreateTantouEditorChangeEscalationRequest
    {
        [Required]
        public Guid SeriesId { get; set; }

        public Guid? RequestedTantouEditorId { get; set; }

        [MaxLength(50)]
        public string? Priority { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = null!;
    }
}
