using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    public class GenreRequest
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;
    }
}
