using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class ProposalPage
    {
        public Guid ProposalPageId { get; set; }
        public Guid SeriesId { get; set; }
        public int PageNo { get; set; }
        public Guid PreviewFileAssetId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Series Series { get; set; } = null!;
        public FileAsset PreviewFileAsset { get; set; } = null!;
    }
}
