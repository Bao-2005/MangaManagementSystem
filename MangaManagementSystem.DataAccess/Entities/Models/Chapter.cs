using MangaManagementSystem.DataAccess.Entities.Enums;
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
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Series Series { get; set; } = null!;
        public ICollection<Manuscript> Manuscripts { get; set; } = new List<Manuscript>();
        public ICollection<PageTask> PageTasks { get; set; } = new List<PageTask>();
    }
}
