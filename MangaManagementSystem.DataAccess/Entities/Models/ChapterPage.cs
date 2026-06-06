using System;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class ChapterPage
    {
        public Guid ChapterPageId { get; set; }

        public Guid ChapterId { get; set; }

        public int PageNo { get; set; }

        public Guid ImageFileAssetId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        public Chapter Chapter { get; set; } = null!;

        public FileAsset ImageFileAsset { get; set; } = null!;
    }
}
