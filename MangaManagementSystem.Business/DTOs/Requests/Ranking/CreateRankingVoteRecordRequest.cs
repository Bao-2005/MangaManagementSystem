using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Ranking
{
    public class CreateRankingVoteRecordRequest
    {
        [Required]
        public Guid SeriesId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Period { get; set; } = null!;

        [Required]
        public int ReaderCount { get; set; }

        [Required]
        public int VoteCount { get; set; }
    }
}
