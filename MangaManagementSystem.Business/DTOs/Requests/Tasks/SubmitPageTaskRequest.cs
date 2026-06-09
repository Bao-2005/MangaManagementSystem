using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Tasks;

public class SubmitPageTaskRequest
{
    [Required]
    public Guid SubmittedFileAssetId { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}
