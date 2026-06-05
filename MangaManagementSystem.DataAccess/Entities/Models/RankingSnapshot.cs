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

        public string Period { get; set; } = null!;
        public int RankNo { get; set; }
        public bool IsBottom20Percent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Series Series { get; set; } = null!;
    }
}
