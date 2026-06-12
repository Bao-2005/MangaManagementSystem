using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Series
{
    public class CreateBoardVoteRequest
    {
        [Required]
        public bool? VoteValue { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
