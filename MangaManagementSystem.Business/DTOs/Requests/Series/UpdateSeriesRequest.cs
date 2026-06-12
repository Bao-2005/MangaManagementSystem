using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class UpdateSeriesRequest
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [StringLength(2000, MinimumLength = 100)]
        public string? Synopsis { get; set; }

        [MaxLength(50)]
        public string? PublicationType { get; set; }
    }
}
