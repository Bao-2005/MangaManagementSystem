using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class CreateEscalationRequest
    {
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string EntityType { get; set; } = null!;

        [Required]
        public Guid EntityId { get; set; }

        [Required]
        public Guid SeriesId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = null!;
    }
}
