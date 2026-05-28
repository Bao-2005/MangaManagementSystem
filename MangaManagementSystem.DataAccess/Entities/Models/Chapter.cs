using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class Chapter
    {
        public Guid ChapterId { get; set; }

        public Guid SeriesId { get; set; }

        public int ChapterNo { get; set; }

        public string Title { get; set; } = null!;

        public int TotalPages { get; set; }

        public DateTime? PublicationDate { get; set; }

        public DateTime? SubmissionDeadline { get; set; }

        public string Status { get; set; } = "Draft";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Series Series { get; set; } = null!;

        public ICollection<Manuscript> Manuscripts { get; set; } = new List<Manuscript>();

        public ICollection<ChapterPage> ChapterPages { get; set; } = new List<ChapterPage>();

        public ICollection<PageTask> PageTasks { get; set; } = new List<PageTask>();
    }
}
