using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    public class UpdateAnnotationRequest
    {
        [Required]
        public string Content { get; set; } = null!;
    }
}
