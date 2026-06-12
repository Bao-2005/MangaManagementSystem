using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class CreateSeriesRequest
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(2000, MinimumLength = 100)]
        public string Synopsis { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string PublicationType { get; set; } = null!;

        public List<Guid> GenreIds { get; set; } = new();

        public Guid? SourceZipFileAssetId { get; set; }

        public List<Guid> SamplePageFileAssetIds { get; set; } = new();
    }
}
