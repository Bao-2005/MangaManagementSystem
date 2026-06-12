using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Chapters
{
    public class CreateChapterPageRequest
    {
        [Required]
        public Guid ChapterId { get; set; }

        [Required]
        public int PageNo { get; set; }

        [Required]
        public Guid ImageFileAssetId { get; set; }
    }
}
