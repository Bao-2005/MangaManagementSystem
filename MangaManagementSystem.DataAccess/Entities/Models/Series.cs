using MangaManagementSystem.DataAccess.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class Series
    {
        public Guid SeriesId { get; set; }
        public Guid MangakaId { get; set; }

        public string Title { get; set; } = null!;
        public string PublicationType { get; set; } = null!;
        public decimal RankingScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public SeriesStatus Status { get; set; }
        public Guid? SourceZipFileAssetId { get; set; }
        public string Synopsis { get; set; } = null!;
        public string? RejectReason { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public User Mangaka { get; set; } = null!;
        public FileAsset? SourceZipFileAsset { get; set; }

        public ICollection<SeriesGenre> SeriesGenres { get; set; } = new List<SeriesGenre>();
        public ICollection<ProposalPage> ProposalPages { get; set; } = new List<ProposalPage>();
        public ICollection<BoardDecision> BoardDecisions { get; set; } = new List<BoardDecision>();
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<VoteRecord> VoteRecords { get; set; } = new List<VoteRecord>();
        public ICollection<RankingSnapshot> RankingSnapshots { get; set; } = new List<RankingSnapshot>();
        public ICollection<Escalation> Escalations { get; set; } = new List<Escalation>();
    }
}
