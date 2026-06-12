using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Manuscripts
{
    public class UpdateManuscriptRequest
    {
        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(1000)]
        public string? Feedback { get; set; }
    }
}
