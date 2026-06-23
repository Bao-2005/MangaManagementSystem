namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class PageTaskReferenceFile
    {
        public Guid PageTaskReferenceFileId { get; set; }
        public Guid PageTaskId { get; set; }
        public Guid FileAssetId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public PageTask PageTask { get; set; } = null!;
        public FileAsset FileAsset { get; set; } = null!;
    }
}
