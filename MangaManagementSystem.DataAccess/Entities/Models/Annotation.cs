using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class Annotation
    {
        public Guid AnnotationId { get; set; }

        public Guid ManuscriptId { get; set; }

        public Guid ChapterPageId { get; set; }

        public Guid AuthorId { get; set; }

        public int PageNo { get; set; }

        public decimal PositionX { get; set; }

        public decimal PositionY { get; set; }

        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Manuscript Manuscript { get; set; } = null!;

        public ChapterPage ChapterPage { get; set; } = null!;

        public User Author { get; set; } = null!;
    }
}
