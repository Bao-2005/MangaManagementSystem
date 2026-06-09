using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class UpdateSeriesRequest
    {
        [MaxLength(150)]
        public string? Title { get; set; }

        [StringLength(2000, MinimumLength = 200)]
        public string? Synopsis { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(1000)]
        public string? RejectReason { get; set; }

        [MaxLength(50)]
        public string? PublicationType { get; set; }
    }
}
