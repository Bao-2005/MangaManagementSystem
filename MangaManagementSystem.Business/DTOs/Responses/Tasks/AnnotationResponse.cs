namespace MangaManagementSystem.Business.DTOs.Responses.Tasks
{
    public class AnnotationResponse
    {
        public Guid AnnotationId { get; set; }
        public Guid ManuscriptId { get; set; }
        public Guid ChapterId { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = null!;
        public int PageNo { get; set; }
        public decimal PositionX { get; set; }
        public decimal PositionY { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
