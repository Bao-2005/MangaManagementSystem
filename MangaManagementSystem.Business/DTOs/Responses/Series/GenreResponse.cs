namespace MangaManagementSystem.Business.DTOs.Responses.Series
{
    public class GenreResponse
    {
        public Guid GenreId { get; set; }
        public string Title { get; set; } = null!;
        public DateTime? DeletedAt { get; set; }
    }
}
