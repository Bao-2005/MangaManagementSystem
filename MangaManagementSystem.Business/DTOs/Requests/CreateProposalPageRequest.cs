using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests
{
    public class CreateProposalPageRequest
    {
        [Required]
        public int PageNo { get; set; }

        [Required]
        public Guid PreviewFileAssetId { get; set; }
    }
}
