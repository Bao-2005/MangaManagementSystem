namespace MangaManagementSystem.Business.DTOs.Responses.Chapters
{
    public class ChapterPageResponse
    {
        public Guid ChapterPageId { get; set; }
        public Guid ChapterId { get; set; }
        public int PageNo { get; set; }
        public Guid ImageFileAssetId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
