using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Tasks
{
    public class CreateAnnotationRequest
    {
        [Required]
        public int PageNo { get; set; }

        [Required]
        public decimal PositionX { get; set; }

        [Required]
        public decimal PositionY { get; set; }

        [Required]
        public string Content { get; set; } = null!;
    }
}
