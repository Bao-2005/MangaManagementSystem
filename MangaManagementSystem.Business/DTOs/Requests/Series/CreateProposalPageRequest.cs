using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class CreateProposalPageRequest
    {
        [Required]
        public int PageNo { get; set; }

        [Required]
        public Guid PreviewFileAssetId { get; set; }
    }
}
