using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class ChapterPage
    {
        public Guid ChapterPageId { get; set; }

        public Guid ChapterId { get; set; }

        public Guid ManuscriptId { get; set; }

        public int PageNo { get; set; }

        public Guid ImageFileAssetId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        public Chapter Chapter { get; set; } = null!;

        public Manuscript Manuscript { get; set; } = null!;

        public FileAsset ImageFileAsset { get; set; } = null!;

        public ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
    }
}
