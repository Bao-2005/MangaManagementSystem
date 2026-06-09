using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class UpdateEscalationRequest
    {
        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(1000)]
        public string? Resolution { get; set; }
    }
}
