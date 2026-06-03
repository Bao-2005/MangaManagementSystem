using MangaManagementSystem.DataAccess.Entities.Enums;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class Manuscript
    {
        public Guid ManuscriptId { get; set; }

        public Guid ChapterId { get; set; }

        public int VersionNo { get; set; }

        public Guid? PreviewFileAssetId { get; set; }

        public Guid? SourceFileAssetId { get; set; }

        public ManuscriptStatus Status { get; set; } = ManuscriptStatus.Submitted;

        public string? Feedback { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Chapter Chapter { get; set; } = null!;

        public FileAsset? PreviewFileAsset { get; set; }

        public FileAsset? SourceFileAsset { get; set; }

        public ICollection<ChapterPage> ChapterPages { get; set; } = new List<ChapterPage>();

        public ICollection<PageTask> PageTasks { get; set; } = new List<PageTask>();

        public ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
    }
}
