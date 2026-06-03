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

        public Guid? TantouEditorId { get; set; }

        public string Title { get; set; } = null!;

        public string Genre { get; set; } = null!;

        public string PublicationType { get; set; } = null!;

        public SeriesStatus Status { get; set; } = SeriesStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    }
}
