using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class RankingSnapshot
    {
        public Guid RankingSnapshotId { get; set; }
        public Guid SeriesId { get; set; }
        public Guid? VoteRecordId { get; set; }

        public string Period { get; set; } = null!;
        public decimal Score { get; set; }
        public int ReaderCount { get; set; }
        public int VoteCount { get; set; }
        public int RankNo { get; set; }
        public bool IsBottom20Percent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Series Series { get; set; } = null!;
        public VoteRecord? VoteRecord { get; set; }
    }
}
