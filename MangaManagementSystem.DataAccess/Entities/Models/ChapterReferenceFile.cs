namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class ChapterReferenceFile
    {
        public Guid ChapterReferenceFileId { get; set; }
        public Guid ChapterId { get; set; }
        public Guid FileAssetId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Chapter Chapter { get; set; } = null!;
        public FileAsset FileAsset { get; set; } = null!;
    }
}
